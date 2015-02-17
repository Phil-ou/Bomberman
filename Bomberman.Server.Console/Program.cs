using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Threading;
using Bomberman.Common;
using Bomberman.Common.DataContracts;
using Bomberman.Common.Randomizer;
using Bomberman.Server.Entities;
using Bomberman.Server.Interfaces;
using Bomberman.Server.PlayerManager;

namespace Bomberman.Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //TimerHelper<object> timer1 = new TimerHelper<object>();
            //timer1.TimerAction += (timer, o) => DisplayHelp();
            //timer1.Start(TimeSpan.FromMilliseconds(500), false, null);
            //Thread.Sleep(100);
            //timer1.Dispose();

            Log.Initialize(ConfigurationManager.AppSettings["logpath"], "bomberman_server.log");
            //
            //List<BonusOccurancy> occurancies = new List<BonusOccurancy>
            //{
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.Empty,
            //        Occurancy = 10
            //    },
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.BonusFireUp,
            //        Occurancy = 17
            //    },
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.BonusFireDown,
            //        Occurancy = 17
            //    },
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.BonusBombUp,
            //        Occurancy = 18
            //    },
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.BonusBombDown,
            //        Occurancy = 18
            //    },
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.BonusBombKick,
            //        Occurancy = 10
            //    },
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.BonusFlameBomb,
            //        Occurancy = 7
            //    },
            //    new BonusOccurancy
            //    {
            //        Value = EntityTypes.BonusNoClipping,
            //        Occurancy = 3
            //    },
            //};
            List<IOccurancy<EntityTypes>> occurancies = new List<IOccurancy<EntityTypes>>
            {
                new BonusOccurancy
                {
                    Value = EntityTypes.BonusBombKick,
                    Occurancy = 50
                },
                new BonusOccurancy
                {
                    Value = EntityTypes.BonusFlameBomb,
                    Occurancy = 50
                },
            };
            //
            PlayerManager.PlayerManager playerManager = new PlayerManager.PlayerManager(4);
            //
            string mapPath = ConfigurationManager.AppSettings["mappath"];
            MapManager.MapManager mapManager = new MapManager.MapManager();
            mapManager.ReadMaps(mapPath);
            //
            EntityMap entityMap = new EntityMap();
            //
            string port = ConfigurationManager.AppSettings["port"];
            WCFHost.WCFHost host = new WCFHost.WCFHost(port, playerManager, (s, callback) => new Player(s, callback));
            
            //
            Server server = new Server(host, playerManager, mapManager, entityMap, occurancies);
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
                            if (cki.Modifiers == ConsoleModifiers.Shift || cki.Modifiers == ConsoleModifiers.Control)
                            {
                                System.Console.WriteLine("#Maps: {0}  Path={1}", mapManager.Maps.Count, mapManager.Path);
                                foreach (Map map in mapManager.Maps.OrderBy(x => x.Description.Id))
                                {
                                    System.Console.WriteLine("MAP{0}|[{1},{2}]:{3} - {4}", map.Description.Id, map.Description.Size, map.Description.Size, map.Description.Title, map.Description.Description);
                                    if (cki.Modifiers == ConsoleModifiers.Control)
                                        DisplayMap(map);
                                }
                            }
                            else
                                DisplayEntityMap(entityMap);
                            break;
                        case ConsoleKey.P:
                            DumpPlayers(playerManager);
                            break;
                        case ConsoleKey.B:
                            System.Console.WriteLine("#Bonus: {0} sum {1}", occurancies.Count, RangeRandom.SumOccurancies(occurancies));
                            foreach(BonusOccurancy occurancy in occurancies)
                                System.Console.WriteLine("BONUS: {0} -> {1}", occurancy.Value, occurancy.Occurancy);
                            break;
                    }
                }
                else
                    Thread.Sleep(100);
            }
        }

        private static void DisplayHelp()
        {
            System.Console.WriteLine("Commands:");
            System.Console.WriteLine("x: Stop server");
            System.Console.WriteLine("m: Dump current map");
            System.Console.WriteLine("shift+m: Dump map description list");
            System.Console.WriteLine("ctrl+m: Dump map list");
            System.Console.WriteLine("p: Dump player list");
            System.Console.WriteLine("b: Dump bonus list");
        }

        private static void DumpPlayers(IPlayerManager playerManager)
        {
            System.Console.WriteLine("#Players: {0}", playerManager.PlayerCount);
            foreach (IPlayer player in playerManager.Players)
            {
                int id = playerManager.GetId(player);
                System.Console.WriteLine("PLAYER: {0}|{1}[{2}] #Bomb: {3} #MaxBomb: {4} position:{5},{6} | {7:HH:mm:ss.fff} {8:HH:mm:ss.fff}", id, player.Name, player.State, player.BombCount, player.MaxBombCount, player.LocationX, player.LocationY, player.LastActionFromClient, player.LastActionToClient);
            }
        }

        #region Display map

        private static void DisplayEntityMap(EntityMap entityMap)
        {
            for (int y = 0; y < entityMap.Size; y++)
            {
                for (int x = 0; x < entityMap.Size; x++)
                {
                    IEntityCell cell = entityMap.GetCell(x, y);
                    EntityTypes entity = cell.Entities.Any() ? cell.Entities.Select(e => e.Type).Aggregate((e, e1) => e | e1) : EntityTypes.Empty;
                    DisplayEntity(entity);
                }
                System.Console.WriteLine();
            }
        }

        private static void DisplayMap(Map map)
        {
            for (int y = 0; y < map.Description.Size; y++)
            {
                for (int x = 0; x < map.Description.Size; x++)
                {
                    EntityTypes entity = map.GetEntity(x, y);
                    DisplayEntity(entity);
                }
                System.Console.WriteLine();
            }
        }

        private static void DisplayEntity(EntityTypes entity)
        {
            ConsoleColor color = ConsoleColor.Gray;
            if (IsFlames(entity))
                color = ConsoleColor.Red;
            if (IsBonus(entity))
                color = ConsoleColor.Green;
            if (IsBomb(entity))
                color = ConsoleColor.Yellow;

            char c = '?';
            if (IsPlayer(entity))
                c = GetPlayer(entity);
            else if (IsBonus(entity))
                c = GetBonus(entity);
            else if (IsBomb(entity))
                c = '*';
            else if (IsWall(entity))
                c = '█';
            else if (IsDust(entity))
                c = '.';
            else if (IsEmpty(entity))
                c = '_';

            System.Console.ForegroundColor = color;
            System.Console.Write(c);

            System.Console.ResetColor();
        }

        private static bool IsEmpty(EntityTypes entity)
        {
            return (entity & EntityTypes.Empty) == EntityTypes.Empty;
        }

        private static bool IsDust(EntityTypes entity)
        {
            return (entity & EntityTypes.Dust) == EntityTypes.Dust;
        }

        private static bool IsWall(EntityTypes entity)
        {
            return (entity & EntityTypes.Wall) == EntityTypes.Wall;
        }

        private static bool IsFlames(EntityTypes entity)
        {
            return (entity & EntityTypes.Flames) == EntityTypes.Flames;
        }

        private static bool IsBomb(EntityTypes entity)
        {
            return (entity & EntityTypes.Bomb) == EntityTypes.Bomb;
        }

        private static bool IsPlayer(EntityTypes entity)
        {
            return ((entity & EntityTypes.Player1) == EntityTypes.Player1
                    || (entity & EntityTypes.Player2) == EntityTypes.Player2
                    || (entity & EntityTypes.Player3) == EntityTypes.Player3
                    || (entity & EntityTypes.Player4) == EntityTypes.Player4);
        }

        private static char GetPlayer(EntityTypes entity)
        {
            if ((entity & EntityTypes.Player1) == EntityTypes.Player1)
                return '1';
            if ((entity & EntityTypes.Player2) == EntityTypes.Player2)
                return '2';
            if ((entity & EntityTypes.Player3) == EntityTypes.Player3)
                return '3';
            if ((entity & EntityTypes.Player4) == EntityTypes.Player4)
                return '4';
            return '\\';
        }

        private static bool IsBonus(EntityTypes entity)
        {
            return (entity & EntityTypes.BonusFireUp) == EntityTypes.BonusFireUp
                   || (entity & EntityTypes.BonusNoClipping) == EntityTypes.BonusNoClipping
                   || (entity & EntityTypes.BonusBombUp) == EntityTypes.BonusBombUp
                   || (entity & EntityTypes.BonusBombKick) == EntityTypes.BonusBombKick
                   || (entity & EntityTypes.BonusFlameBomb) == EntityTypes.BonusFlameBomb
                   || (entity & EntityTypes.BonusFireDown) == EntityTypes.BonusFireDown
                   || (entity & EntityTypes.BonusBombDown) == EntityTypes.BonusBombDown
                   || (entity & EntityTypes.BonusH) == EntityTypes.BonusH
                   || (entity & EntityTypes.BonusI) == EntityTypes.BonusI
                   || (entity & EntityTypes.BonusJ) == EntityTypes.BonusJ;
        }

        private static char GetBonus(EntityTypes entity)
        {
            if ((entity & EntityTypes.BonusFireUp) == EntityTypes.BonusFireUp)
                return 'a';
            if ((entity & EntityTypes.BonusNoClipping) == EntityTypes.BonusNoClipping)
                return 'b';
            if ((entity & EntityTypes.BonusBombUp) == EntityTypes.BonusBombUp)
                return 'c';
            if ((entity & EntityTypes.BonusBombKick) == EntityTypes.BonusBombKick)
                return 'd';
            if ((entity & EntityTypes.BonusFlameBomb) == EntityTypes.BonusFlameBomb)
                return 'e';
            if ((entity & EntityTypes.BonusFireDown) == EntityTypes.BonusFireDown)
                return 'f';
            if ((entity & EntityTypes.BonusBombDown) == EntityTypes.BonusBombDown)
                return 'g';
            if ((entity & EntityTypes.BonusH) == EntityTypes.BonusH)
                return 'h';
            if ((entity & EntityTypes.BonusI) == EntityTypes.BonusI)
                return 'i';
            if ((entity & EntityTypes.BonusJ) == EntityTypes.BonusJ)
                return 'j';
            return '/';
        }

        #endregion
    }
}
