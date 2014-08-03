using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Common;

namespace Bomberman.Server.Console
{
    class Program
    {
        private static void DisplayHelp()
        {
            System.Console.WriteLine("Commands:");
            System.Console.WriteLine("x: Stop server");
        }

        static void Main(string[] args)
        {
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], "bomberman_server.log");

            PlayerManager playerManager = new PlayerManager(4);

            string port = ConfigurationManager.AppSettings["port"];
            WCFHost host = new WCFHost(port, playerManager, (s, callback) => new Player(s, callback));
            Server server = new Server(host, playerManager);

            server.Start();

            bool stopped = false;
            while (!stopped)
            {
                if (System.Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = System.Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        default:
                            DisplayHelp();
                            break;
                        case ConsoleKey.X:
                            server.Stop();
                            stopped = true;
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(100);
            }
        }
    }
}
