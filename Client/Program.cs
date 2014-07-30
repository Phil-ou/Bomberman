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
        //public static IBombermanService Proxy
        //{
        //    get
        //    {
        //        var context = new InstanceContext(new BombermanCallbackService());
        //        var toto = new DuplexChannelFactory<IBombermanService>(context, "WSDualHttpBinding_IBombermanService");
        //        IBombermanService toto2 = null;
        //        Task t = Task.Factory.StartNew(() => {
        //                                        toto2 = toto.CreateChannel();
        //        });
        //        Task.WaitAll(t);
        //        return toto2;
        //    }
        //}

        public static IBombermanService Proxy { get; set; }

        static void Main(string[] args)
        {
            var context = new InstanceContext(new BombermanCallbackService());
            var toto = new DuplexChannelFactory<IBombermanService>(context, "WSDualHttpBinding_IBombermanService");
            Proxy = toto.CreateChannel();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("-------- Welcome to Bomberman --------");
            Console.WriteLine("--------------------------------------\n\n");
            Console.WriteLine("Type your player name :\n");
            string login = Console.ReadLine();
            ConnectPlayer(login);
            Log.Initialize(@"D:\Temp\BombermanLogs", "Client_" + login +".log");
            Log.WriteLine(Log.LogLevels.Info, "Logged at " + DateTime.Now.ToShortTimeString());
            while (true)
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

                }
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
