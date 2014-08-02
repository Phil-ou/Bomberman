using System;
using System.ServiceModel;
using Common.DataContract;
using Common.Interfaces;
using Common.Log;

namespace Client
{
    //TODO !! var/params names
    class Program
    {
        public static IBombermanService Proxy { get; private set; }

        public const string MapPath = @"C:\Users\hisil\HDD backup 2012\Bomberman\Server\map.dat";

        static void Main()
        {
            string username = "";
            int id;
            var context = new InstanceContext(new BombermanCallbackService());
            var factory = new DuplexChannelFactory<IBombermanService>(context, "WSDualHttpBinding_IBombermanService");
            Proxy = factory.CreateChannel();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("-------- Welcome to Bomberman --------");
            Console.WriteLine("--------------------------------------\n\n");
            Console.WriteLine("Type your player name :\n");
            string login = Console.ReadLine();
            username = login;
            id = Guid.NewGuid().GetHashCode();
            ConnectPlayer(username, id);
            Log.Initialize(@"D:\Temp\BombermanLogs", "Client_" + login +".log");
            Log.WriteLine(Log.LogLevels.Info, "Logged at " + DateTime.Now.ToShortTimeString());

            bool stop = false;
            while (!stop)
            {
                ConsoleKeyInfo keyboard = Console.ReadKey();
                switch (keyboard.Key)
                {
                    //s
                    case ConsoleKey.S:
<<<<<<< HEAD
                        StartGame();
=======
<<<<<<< HEAD
                        //Console.WriteLine("\nEnter the path of the map.bat");
                        StartGame(@"C:\Users\hisil\HDD backup 2012\Bomberman\Server\map.bat");
=======
                        Console.WriteLine("\nEnter the path of the map.bat");
                        StartGame(Console.ReadLine());
>>>>>>> origin/master
>>>>>>> origin/master
                        break;
                    case ConsoleKey.UpArrow:
                        MoveTo(id, ActionType.MoveUp);
                        break;
                    case ConsoleKey.LeftArrow:
                        MoveTo(id, ActionType.MoveLeft);
                        break;
                    case ConsoleKey.RightArrow:
                        MoveTo(id, ActionType.MoveRight);
                        break;
                    case ConsoleKey.DownArrow:
                        MoveTo(id, ActionType.MoveDown);
                        break;
                    case ConsoleKey.X: // SinaC: never leave a while(true) without an exit condition
                        stop = true;
                        break;
                }
            }

            // SinaC: Clean properly factory
            try
            {
                factory.Close();
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Exception:{0}", ex);
                factory.Abort();
            }
        }

        //todo replace playername by an id ...
        private static void ConnectPlayer(string username, int id)
        {
            Proxy.ConnectUser(username, id);
        }

        private static void StartGame()
        {
            Proxy.StartGame(MapPath);
        }

        private static void MoveTo(int playerId, ActionType actionType)
        {
            Proxy.MovePlayerToLocation(playerId, actionType);
        }
    }
}
