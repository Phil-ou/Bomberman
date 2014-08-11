using System;
using System.Linq;
using System.Configuration;
using Bomberman.Common;
using Bomberman.Common.DataContracts;
using Bomberman.Server.Console.Entities;
using Bomberman.Server.Console.Interfaces;

namespace Bomberman.Server.Console
{
    class Program
    {
        private static void DisplayHelp()
        {
            System.Console.WriteLine("Commands:");
            System.Console.WriteLine("x: Stop server");
            System.Console.WriteLine("m: Dump map list");
            System.Console.WriteLine("p: Dump player list");
        }

        static void Main(string[] args)
        {
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], "bomberman_server.log");
            //
            PlayerManager playerManager = new PlayerManager(4);
            //
            string mapPath = ConfigurationManager.AppSettings["mappath"];
            MapManager mapManager = new MapManager();
            mapManager.ReadMaps(mapPath);
            //
            EntityMap entityMap = new EntityMap();
            //
            string port = ConfigurationManager.AppSettings["port"];
            WCFHost host = new WCFHost(port, playerManager, (s, callback) => new Player(s, callback));
            
            //
            Server server = new Server(host, playerManager, mapManager, entityMap);
            //
            server.Start();
            //
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
                        case ConsoleKey.M:
                            System.Console.WriteLine("#Maps: {0}  Path={1}", mapManager.Maps.Count, mapManager.Path);
                            foreach(MapDescription map in mapManager.MapDescriptions.OrderBy(x => x.Id))
                                System.Console.WriteLine("MAP{0}|[{1},{2}]:{3} - {4}", map.Id, map.Size, map.Size, map.Title, map.Description);
                            break;
                        case ConsoleKey.P:
                            System.Console.WriteLine("#Players: {0}", playerManager.PlayerCount);
                            foreach(IPlayer player in playerManager.Players)
                                System.Console.WriteLine("PLAYER: {0}[{1}] #Bomb: {2} #MaxBomb: {3} position:{4},{5}", player.Name, player.State, player.BombCount, player.MaxBombCount, player.LocationX, player.LocationY);
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(100);
            }
        }
    }
}
