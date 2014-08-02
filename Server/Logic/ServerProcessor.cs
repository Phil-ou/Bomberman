using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Common.DataContract;
using Common.Interfaces;
using Server.Model;

namespace Server.Logic
{
    public class ServerProcessor
    {
        #region variables

        private const int MapSize = 10;

        private ServerModel _server;

        #endregion variables

        #region methods

        public void StartServer()
        {
            _server = new ServerModel();
            _server.Initialize();
        }
        //id is generated client side to allow many players with same username
        public void ConnectUser(string username, int id)
        {
            //create new Player
            PlayerModel newPlayer = new PlayerModel
                {
                    Player = new Player
                    {
                        Id = id,
                        Username = username,
                        //Check if its the first user to be connected
                        IsCreator = _server.PlayersOnline.Count == 0
                    },
                    CallbackService = OperationContext.Current.GetCallbackChannel<IBombermanCallbackService>()
                };
            //register user to the server
            _server.PlayersOnline.Add(newPlayer);
            //create a list of login to send to client
            List<string> playersNamesList = _server.PlayersOnline.Select(x => x.Player.Username).ToList();
            //Warning players that a new player is connected and send them the list of all players online
            bool canStartGame = _server.PlayersOnline.Count > 1; // SinaC: no need to compute this in inner loop
            foreach (PlayerModel player in _server.PlayersOnline)
            {
                //todo check if player disconnect
                player.CallbackService.OnUserConnected(newPlayer.Player, playersNamesList, canStartGame);
            }
        }

        public void StartGame(string mapPath)
        {
            //create the list of players to pass to client
            List<Player> players = _server.PlayersOnline.Select(playerModel => playerModel.Player).ToList();

            Game newGame = new Game
                {
                    Map = new Map
                        {
                            MapName = "Custom Map",
                            GridPositions = GenerateGrid(players, mapPath)
                        },
                    CurrentStatus = GameStatus.Started,
                };
            _server.GameCreated = newGame;
            //send the game firt and last time to all players
            foreach (PlayerModel currentPlayer in _server.PlayersOnline)
            {
                currentPlayer.CallbackService.OnGameStarted(newGame);
            }
        }

        public void MovePlayerToLocation(int idPlayer, ActionType actionType)
        {
            foreach (Player player in _server.GameCreated.Map.GridPositions.Where(livingObject => livingObject is Player && ((Player)livingObject).Id == idPlayer))
            {
                switch (actionType)
                {
                    case ActionType.MoveUp:
                        Move(player, 0, -1);
                        break;
                    case ActionType.MoveDown:
                        Move(player, 0, +1);
                        break;
                    case ActionType.MoveRight:
                        Move(player, +1, 0);
                        break;
                    case ActionType.MoveLeft:
                        Move(player, -1, 0);
                        break;
                }
                break;
            }
        }

        private List<LivingObject> GenerateGrid(List<Player> players,string mapPath)
        {
            List<LivingObject> matrice = new List<LivingObject>();

            using (StreamReader reader = new StreamReader(mapPath, Encoding.UTF8))
            {
                string objectsToAdd = reader.ReadToEnd().Replace("\n", "").Replace("\r", "");

                for (int y = 0; y < MapSize; y++)
                {
                    for (int x = 0; x < MapSize; x++)
                    {
                        LivingObject livingObject = null;
                        char cell = objectsToAdd[(y * MapSize) + x]; // SinaC: factoring is the key :)   y and x were inverted
                        switch (cell)
                        {
                            case 'u':
                                livingObject = new Wall
                                    {
                                        WallType = WallType.Undestructible,
                                        ObjectPosition = new Position
                                            {
                                                PositionX = x,
                                                PositionY = y
                                            }
                                    };
                                break;
                            case 'd':
                                livingObject = new Wall
                                    {
                                        WallType = WallType.Destructible,
                                        ObjectPosition = new Position
                                            {
                                                PositionX = x,
                                                PositionY = y
                                            }
                                    };
                                break;
                                //case 'b' :
                                //    currentlivingObject = new Bonus
                                //    {

                                //    };
                                //    break;
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                                if (players.Count > (int) Char.GetNumericValue(cell))
                                {
                                    livingObject = players[(int) Char.GetNumericValue(cell)];
                                    livingObject.ObjectPosition = new Position
                                    {
                                        PositionX = x,
                                        PositionY = y
                                    };

                                }
                                break;
                        }
                        if (livingObject != null)
                            matrice.Add(livingObject);
                    }
                }
            }
            return matrice;
        }


        //// SinaC: Move(0,-1) for MoveUp, Move(0,+1) for MoveDown, ...
        //private void Move(Player before, int stepX, int stepY)
        //{
        //    // Get object at future player location
        //    LivingObject collider = _server.GameCreated.Map.GridPositions.FirstOrDefault(x => before.ObjectPosition.PositionY + stepY == x.ObjectPosition.PositionY
        //                                                                                     && before.ObjectPosition.PositionX + stepX == x.ObjectPosition.PositionX);
        //    // Can't go thru wall
        //    if (collider is Wall)
        //        return;
        //    //can't go if another player is already on the way
        //    if (collider is Player)
        //        return;
        //    Player after = new Player
        //    {
        //        Username = before.Username,
        //        ObjectPosition = new Position
        //        {
        //            PositionX = before.ObjectPosition.PositionX + stepX,
        //            PositionY = before.ObjectPosition.PositionY + stepY,
        //        },
        //        Id = before.Id
        //    };

        //    // Remove player from old position
        //    _server.GameCreated.Map.GridPositions.Remove(before);
        //    // And add to new position
        //    _server.GameCreated.Map.GridPositions.Add(after);

        //    // Send new player position to players
        //    foreach (PlayerModel playerModel in _server.PlayersOnline)
        //        playerModel.CallbackService.OnMove(before, after);
        //}
        // SinaC: Move(0,-1) for MoveUp, Move(0,+1) for MoveDown, ...
        // alternative to bypass the erase in window console
        private void Move(Player before, int stepX, int stepY)
        {
            // Get object at future player location
            LivingObject collider = _server.GameCreated.Map.GridPositions.FirstOrDefault(x => before.ObjectPosition.PositionY + stepY == x.ObjectPosition.PositionY
                                                                                             && before.ObjectPosition.PositionX + stepX == x.ObjectPosition.PositionX);
            Player after;
            // Can't go thru wall or player
            if (!(collider is Wall || collider is Player))
            {
                after = new Player
                {
                    Username = before.Username,
                    ObjectPosition = new Position
                    {
                        PositionX = before.ObjectPosition.PositionX + stepX,
                        PositionY = before.ObjectPosition.PositionY + stepY,
                    },
                    Id = before.Id
                };
            }
            else after = before;
            // Remove player from old position
            _server.GameCreated.Map.GridPositions.Remove(before);
            // And add to new position
            _server.GameCreated.Map.GridPositions.Add(after);

            // Send new player position to players
            foreach (PlayerModel playerModel in _server.PlayersOnline)
                playerModel.CallbackService.OnMove(before, after);
        }
        #endregion methods
    }
}
