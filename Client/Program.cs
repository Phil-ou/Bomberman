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

        static void Main(string[] args)
        {
            var context = new InstanceContext(new BombermanCallbackService());
            var factory = new DuplexChannelFactory<IBombermanService>(context, "WSDualHttpBinding_IBombermanService");
            Proxy = factory.CreateChannel();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("-------- Welcome to Bomberman --------");
            Console.WriteLine("--------------------------------------\n\n");
            Console.WriteLine("Type your player name :\n");
            string login = Console.ReadLine();
            ConnectPlayer(login);
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
                        StartGame();
                        break;
                    case ConsoleKey.UpArrow:
                        MoveTo(ActionType.MoveUp, login);
                        break;
                    case ConsoleKey.LeftArrow:
                        MoveTo(ActionType.MoveLeft, login);
                        break;
                    case ConsoleKey.RightArrow:
                        MoveTo(ActionType.MoveRight, login);
                        break;
                    case ConsoleKey.DownArrow:
                        MoveTo(ActionType.MoveDown, login);
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
        private static void ConnectPlayer(string playerName)
        {
            Proxy.ConnectUser(playerName);
        }

        private static void StartGame()
        {
            Proxy.StartGame();
        }
        //todo replace playername by an id ...
        private static void MoveTo(ActionType actionType, string login)
        {
            Proxy.MovePlayerToLocation(login, actionType);
        }
    }
}
