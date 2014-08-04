using System;
using System.Configuration;
using Bomberman.Common;

namespace Bomberman.Client.Console
{
    class Program
    {
        private static void Main(string[] args)
        {
            string name = "BOMBERMANCONSOLE_" + Guid.NewGuid().ToString().Substring(0, 5);
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], name + ".log");
            Client client = new Client();

            string baseAddress = ConfigurationManager.AppSettings["address"];
            WCFProxy proxy = new WCFProxy(client, baseAddress);

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
                            client.Connect(proxy, name);
                            break;
                            // Chat
                        case ConsoleKey.Z:
                            client.Chat("Chat sample :)");
                            break;
                            // Start
                        case ConsoleKey.S:
                            client.StartGame(0);
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(100);
            }
        }
    }
}
