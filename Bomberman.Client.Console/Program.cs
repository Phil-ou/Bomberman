using System;
using System.Configuration;
using Bomberman.Common;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string name = "BOMBERMANCONSOLE_" + Guid.NewGuid().ToString().Substring(0, 5);
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], name + ".log");

            ConsoleUI ui = new ConsoleUI();

            Client client = new Client();

            client.LoginHandler += ui.OnLogin;
            client.UserConnectedHandler += ui.OnUserConnected;
            client.UserDisconnectedHandler += ui.OnUserDisconnected;
            client.GameStartedHandler += ui.OnGameStarted;
            client.BonusPickedUpHandler += ui.OnBonusPickedUp;
            client.ChatReceivedHandler += ui.OnChatReceived;
            client.EntityAddedHandler += ui.OnEntityAdded;
            client.EntityDeletedHandler += ui.OnEntityDeleted;
            client.EntityMovedHandler += ui.OnEntityMoved;
            client.EntityTransformedHandler += ui.OnEntityTransformed;
            client.MultipleEntityModifiedHandler += ui.Redraw;
            client.GameDrawHandler += ui.OnGameDraw;
            client.GameLostHandler += ui.OnGameLost;
            client.GameWonHandler += ui.OnGameWon;
            client.KilledHandler += ui.OnKilled;
            client.ConnectionLostHandler += ui.OnConnectionLost;

            string baseAddress = ConfigurationManager.AppSettings["address"];
            WCFProxy.WCFProxy proxy = new WCFProxy.WCFProxy(client, baseAddress);

            bool stopped = false;
            while (!stopped)
            {
                if (System.Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = System.Console.ReadKey(true);
                    switch (cki.Key)
                    {
                            //
                        case ConsoleKey.X:
                            stopped = true;
                            break;
                            // Connect
                        case ConsoleKey.C:
                            client.Login(proxy, name);
                            break;
                            // Chat
                        case ConsoleKey.Z:
                            client.Chat("Chat sample :)");
                            break;
                            // Start
                        case ConsoleKey.S:
                            client.StartGame(1);
                            break;
                            // Direction
                        case ConsoleKey.UpArrow:
                            client.Move(Directions.Up);
                            break;
                        case ConsoleKey.RightArrow:
                            client.Move(Directions.Right);
                            break;
                        case ConsoleKey.DownArrow:
                            client.Move(Directions.Down);
                            break;
                        case ConsoleKey.LeftArrow:
                            client.Move(Directions.Left);
                            break;
                            // Bomb
                        case ConsoleKey.Spacebar:
                            client.PlaceBomb();
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(100);
            }

            client.Stop();
        }
    }
}
