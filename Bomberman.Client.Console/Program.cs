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

            client.LoggedOn += ui.OnLoggedOn;
            client.UserConnected += ui.OnUserConnected;
            client.UserDisconnected += ui.OnUserDisconnected;
            client.GameStarted += ui.OnGameStarted;
            client.BonusPickedUp += ui.OnBonusPickedUp;
            client.ChatReceived += ui.OnChatReceived;
            client.EntityAdded += ui.OnEntityAdded;
            client.EntityDeleted += ui.OnEntityDeleted;
            client.EntityMoved += ui.OnEntityMoved;
            client.EntityTransformed += ui.OnEntityTransformed;
            client.MultipleEntityModified += ui.OnRedraw;
            client.GameDraw += ui.OnGameDraw;
            client.GameLost += ui.OnGameLost;
            client.GameWon += ui.OnGameWon;
            client.Killed += ui.OnKilled;
            client.ConnectionLost += ui.OnConnectionLost;

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
