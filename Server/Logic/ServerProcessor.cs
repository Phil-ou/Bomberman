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

        public void ConnectUser(Player user)
        {
            //Check if its the first user to be connected
            user.IsCreator = _server.PlayersOnline.Count == 0;
            //create new Player
            PlayerModel newPlayer = new PlayerModel
                {
                    Player = user,
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
                player.CallbackService.OnUserConnected(user, playersNamesList, canStartGame);
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
            //send the game to all players
            foreach (PlayerModel currentPlayer in _server.PlayersOnline)
            {
                currentPlayer.CallbackService.OnGameStarted(newGame);
            }
        }

        public void MovePlayerToLocation(string login, ActionType actionType)
        {
            Player currentPlayer = null; // no need to use a LivingObject if we cast it to Player
            //find the player to move and initialize the current position
            foreach (var item in _server.GameCreated.Map.GridPositions.Where(x => x is Player))
            {
                currentPlayer = item as Player;
                if (currentPlayer != null && currentPlayer.Username == login)
                    break;
            }
            // SinaC: previous loop should be replaced with this Linq query
            //  previous loop can lead to false result if login is not found in GridPositions, currentPlayer would be equal to last player in GridPositions
            //Player currentPlayer = Server.GameCreated.Map.GridPositions.Where(x => x is Player).Cast<Player>().FirstOrDefault(x => x.Username == login);
            switch (actionType)
            {
                case ActionType.MoveUp:
                    //MoveUp(currentPlayer);
                    Move(currentPlayer, 0, -1);
                    break;
                case ActionType.MoveDown:
                    //MoveDown(currentPlayer);
                    Move(currentPlayer, 0, +1);
                    break;
                case ActionType.MoveRight:
                    //MoveRight(currentPlayer);
                    Move(currentPlayer, +1, 0);
                    break;
                case ActionType.MoveLeft:
                    //MoveLeft(currentPlayer);
                    Move(currentPlayer, -1, 0);
                    break;
            }
        }

        private List<LivingObject> GenerateGrid(List<Player> players,string mapPath) // SinaC: path to map should be an additional parameter
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


        // SinaC: Move(0,-1) for MoveUp, Move(0,+1) for MoveDown, ...
        private void Move(Player before, int stepX, int stepY)
        {
            // Get object at future player location
            LivingObject collider = _server.GameCreated.Map.GridPositions.FirstOrDefault(x => before.ObjectPosition.PositionY + stepY == x.ObjectPosition.PositionY
                                                                                             && before.ObjectPosition.PositionX + stepX == x.ObjectPosition.PositionX);
            // Can't go thru wall
            if (collider is Wall)
                return;

            // Update position by creating a new player
            Player after = new Player
            {
                Username = before.Username,
                ObjectPosition = new Position
                {
                    PositionX = before.ObjectPosition.PositionX + stepX,
                    PositionY = before.ObjectPosition.PositionY + stepY,
                }
            };

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
