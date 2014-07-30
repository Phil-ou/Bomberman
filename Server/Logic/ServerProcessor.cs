using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private static int MapSize = 10;

        private static ServerModel Server;

        #endregion variables

        #region methods

        public void StartServer()
        {
            Server = new ServerModel();
            Server.Initialize();
        }

        public void ConnectUser(string login)
        {
            bool canStartGame;
            //create new Player
            PlayerModel newPlayer = new PlayerModel
            {
                CallbackService = OperationContext.Current.GetCallbackChannel<IBombermanCallbackService>(),
                Login = login,
                IsCreator = Server.PlayersOnline.Count == 0
            };
            //register user to the server
            Server.PlayersOnline.Add(newPlayer);
            //create a list of login to send to client
            List<string> logins = Server.PlayersOnline.Select(x => x.Login).ToList();
            //Warning players that a new player is connected and send them the list of all players online
            foreach (PlayerModel currentPlayer in Server.PlayersOnline)
            {
                canStartGame = Server.PlayersOnline.Count > 1 ;
                //todo check if player disconnect
                currentPlayer.CallbackService.OnUserConnected(login, logins, currentPlayer.IsCreator, canStartGame);
            }
        }

        public void StartGame()
        {
            //create the list of players to pass to client
            List<Player> players = Server.PlayersOnline.Select(playerModel => new Player
            {
                Username = playerModel.Login
            }).ToList();

            Game newGame = new Game
            {
                Map = new Map
                {
                    MapName = "Custom Map",
                    GridPositions = GenerateGrid(players)
                },
                CurrentStatus = GameStatus.Started,
            };
            Server.GameCreated = newGame;
            foreach (PlayerModel currentPlayer in Server.PlayersOnline)
            {
                currentPlayer.CallbackService.OnGameStarted(newGame, currentPlayer.Login);
            }
        }

        public void MovePlayerToLocation(string login, ActionType actionType)
        {
            LivingObject currentPlayer = null;
            //find the player to move and initialize the current position
            foreach (var item in Server.GameCreated.Map.GridPositions.Where(x => x is Player))
            {
                currentPlayer =  item as Player;
                if (currentPlayer != null && ((Player)currentPlayer).Username == login)
                {
                    break;
                }
            }
            switch (actionType)
            {
                case ActionType.MoveUp:
                    MoveUp((Player)currentPlayer);
                    break;
                case ActionType.MoveDown:
                    MoveDown((Player) currentPlayer);
                    break;
                //case ConsoleKey.LeftArrow:
                //    break;
                //case ConsoleKey.RightArrow:
                //    break;
            }
        }

        private List<LivingObject> GenerateGrid(List<Player> players)
        {
            List<LivingObject> matrice = new List<LivingObject>();

            using (StreamReader reader = new StreamReader(@"C:\Users\AHF503\Documents\Visual Studio 2012\Projects\Bomberman\Server\Map.dat", Encoding.UTF8))
            {
                string objectsToAdd = reader.ReadToEnd().Replace("\n", "").Replace("\r", "");

                for (int y = 0; y < MapSize; y++)
                {
                    for (int x = 0; x < MapSize; x++)
                    {
                        LivingObject currentlivingObject = null;
                        switch (objectsToAdd[(x * MapSize) + y])
                        {
                            case 'u':
                                currentlivingObject = new Wall
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
                                currentlivingObject = new Wall
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
                                if (players.Count > (int) Char.GetNumericValue(objectsToAdd[(x*MapSize) + y]))
                                {
                                    currentlivingObject = new Player
                                    {
                                        Username = players[(int)Char.GetNumericValue(objectsToAdd[(x * MapSize) + y])].Username,
                                        ObjectPosition = new Position
                                        {
                                            PositionX = x,
                                            PositionY = y
                                        }
                                    };
                                }
                                break;
                        }
                        if (currentlivingObject != null)
                            matrice.Add(currentlivingObject);
                    }
                }
            }
            return matrice;
        }

        private void MoveUp(Player currentPlayer)
        {
            LivingObject currentPlayerBefore = new Player
            {
                ObjectPosition = currentPlayer.ObjectPosition,
                Username = currentPlayer.Username
            };
            LivingObject currentPlayerAfter = currentPlayer;
            //retreive object positionned just above the current player position
            LivingObject objectToNextPosition = Server.GameCreated.Map.GridPositions.FirstOrDefault(x => x.ObjectPosition.PositionY + 1 == currentPlayer.ObjectPosition.PositionY
                                                                                                         && x.ObjectPosition.PositionX == currentPlayer.ObjectPosition.PositionX);
            //if its a Wall then return
            if (objectToNextPosition is Wall) return;
            //modify the currentMap
            currentPlayerAfter.ObjectPosition.PositionY = currentPlayerBefore.ObjectPosition.PositionY - 1;
            //warn each player of the move
            foreach (PlayerModel playerModel in Server.PlayersOnline)
            {
                playerModel.CallbackService.OnMove(currentPlayerBefore, currentPlayerAfter);
            }
        }

        private void MoveDown(Player currentPlayerBefore)
        {
            LivingObject currentPlayerAfter = new Player
            {
                ObjectPosition = new Position
                {
                    PositionX = currentPlayerBefore.ObjectPosition.PositionX,
                    PositionY = currentPlayerBefore.ObjectPosition.PositionY
                },
                Username = currentPlayerBefore.Username
            };
            //retreive object positionned just above the current player position
            LivingObject objectToNextPosition = Server.GameCreated.Map.GridPositions.FirstOrDefault(x => x.ObjectPosition.PositionY - 1 == currentPlayerBefore.ObjectPosition.PositionY
                                                                                                         && x.ObjectPosition.PositionX == currentPlayerBefore.ObjectPosition.PositionX);
            //if its a Wall then return
            if (objectToNextPosition is Wall) return;
            //modify the currentMap
            currentPlayerAfter.ObjectPosition.PositionY = currentPlayerBefore.ObjectPosition.PositionY + 1;
            //warn each player of the move
            foreach (PlayerModel playerModel in Server.PlayersOnline)
            {
                playerModel.CallbackService.OnMove(currentPlayerBefore, currentPlayerAfter);
            }
        }

        #endregion methods
    }
}
