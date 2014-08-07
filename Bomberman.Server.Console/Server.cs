using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bomberman.Common;
using Bomberman.Common.DataContracts;
using Bomberman.Server.Console.Interfaces;

// TODO: 
//  bomb push

namespace Bomberman.Server.Console
{
    public enum ServerStates
    {
        WaitStartGame,
        GameStarted,
    }

    public class Server
    {
        private const int MaxExplosionRange = 1;
        private const int BombTimer = 3000; // in ms
        private const int FlameTimer = 500; // in ms

        private static readonly int[] BombNeighboursX = {-1, 1, 0, 0};
        private static readonly int[] BombNeighboursY = {0, 0, -1, 1};

        private readonly WCFHost _host;
        private readonly IPlayerManager _playerManager;
        private readonly IMapManager _mapManager;

        private CancellationTokenSource _cancellationTokenSource;

        private int _timerTableSize;
        private int[] _bombTimerTable; // store timer (in ms) for each map cell (-1: no timer)
        private int[] _flameTimerTable; // store timer (in ms) for each map cell (-1: no timer)
        private int[] _bonusTimerTable; // store timer (in ms) for each map cell (-1: no timer)
        private Task _timerTableTask;

        public ServerStates State { get; private set; }
        public Map GameMap { get; private set; }
        public int PlayersInGameCount { get; private set; }

        public Server(WCFHost host, IPlayerManager playerManager, IMapManager mapManager)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");

            _host = host;
            _playerManager = playerManager;
            _mapManager = mapManager;

            _host.OnLogin += OnLogin;
            _host.OnLogout += OnLogout;
            _host.OnStartGame += OnStartGame;
            _host.OnChangeDirection += OnChangeDirection;
            _host.OnMove += OnMove;
            _host.OnPlaceBomb += OnPlaceBomb;
            _host.OnChat += OnChat;

            State = ServerStates.WaitStartGame;
        }

        public void Start()
        {
            _host.Start();

            _cancellationTokenSource = new CancellationTokenSource();
            _timerTableTask = Task.Factory.StartNew(TimerTableTask, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _host.Stop();
            _timerTableTask.Wait(1000);
        }

        private void OnLogin(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "New player {0}|{1} connected", player.Name, playerId);

            EntityTypes playerEntity = EntityTypes.Empty;
            switch (playerId)
            {
                case 0:
                    playerEntity = EntityTypes.Player1;
                    break;
                case 1:
                    playerEntity = EntityTypes.Player2;
                    break;
                case 2:
                    playerEntity = EntityTypes.Player3;
                    break;
                case 3:
                    playerEntity = EntityTypes.Player4;
                    break;
            }
            player.PlayerEntity = playerEntity;

            // Inform player about its playerId
            player.OnLogin(LoginResults.Successful, playerId, playerEntity, _mapManager.MapDescriptions);

            // Inform other player about new player
            foreach (IPlayer other in _playerManager.Players.Where(x => x != player))
                other.OnUserConnected(player.Name, playerId);

            // TODO: if game is started, handle differently
        }

        private void OnLogout(IPlayer player)
        {
            throw new NotImplementedException();
        }

        private void OnStartGame(IPlayer player, int mapId)
        {
            Log.WriteLine(Log.LogLevels.Info, "Start game {0} map {1}", player.Name, mapId);

            if (State == ServerStates.WaitStartGame)
            {
                Map map = _mapManager.Maps.FirstOrDefault(x => x.Description.Id == mapId);
                if (map != null)
                {
                    // Clone map
                    GameMap = new Map
                        {
                            Description = map.Description,
                            MapAsArray = (EntityTypes[]) map.MapAsArray.Clone()
                        };

                    bool positionError = false;
                    // Set players position
                    foreach (IPlayer p in _playerManager.Players)
                    {
                        var entry = GameMap.MapAsArray.Select((entity, index) => new
                            {
                                entity,
                                index
                            }).FirstOrDefault(x => x.entity == p.PlayerEntity);
                        if (entry != null)
                        {
                            // Set player position
                            int x, y;
                            GameMap.GetLocation(entry.index, out x, out y);
                            p.LocationX = x;
                            p.LocationY = y;
                        }
                        else
                        {
                            Log.WriteLine(Log.LogLevels.Error, "Cannot find position of {0} in map {1}", player.PlayerEntity, GameMap.Description.Id);
                            positionError = true;
                            break;
                        }
                    }

                    // Replace unused player with dust
                    for (int i = 0; i < GameMap.MapAsArray.Length; i++)
                    {
                        EntityTypes entity = GameMap.MapAsArray[i];
                        if (IsPlayer(entity))
                        {
                            EntityTypes playerEntity = GetPlayer(entity);
                            if (playerEntity != EntityTypes.Empty)
                            {
                                // Search player in player list
                                bool playerFound = _playerManager.Players.Any(x => x.PlayerEntity == playerEntity);
                                if (!playerFound)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "Replace unused player position {0} with dust at index {1}", playerEntity, i);
                                    GameMap.MapAsArray[i] = EntityTypes.Dust;
                                }
                            }
                            else
                                Log.WriteLine(Log.LogLevels.Warning, "Inconsistant player position at index {0} in map {1}", i, GameMap.Description.Id);
                        }
                    }

                    // If no problem while positioning player, let's go
                    if (!positionError)
                    {
                        // TODO:

                        // Create timer maps
                        _timerTableSize = GameMap.Description.Size*GameMap.Description.Size;
                        _bombTimerTable = new int[_timerTableSize];
                        _flameTimerTable = new int[_timerTableSize];
                        _bonusTimerTable = new int[_timerTableSize];
                        for (int i = 0; i < _timerTableSize; i++)
                        {
                            _bombTimerTable[i] = -1;
                            _flameTimerTable[i] = -1;
                            _bonusTimerTable[i] = -1;
                        }

                        // Change player state
                        foreach (IPlayer p in _playerManager.Players)
                            p.State = PlayerStates.Playing;

                        //
                        PlayersInGameCount = _playerManager.Players.Count(x => x.State == PlayerStates.Playing);

                        // Inform players about game started
                        foreach (IPlayer p in _playerManager.Players)
                            p.OnGameStarted(p.LocationX, p.LocationY, GameMap);

                        State = ServerStates.GameStarted;
                    }
                    else
                        Log.WriteLine(Log.LogLevels.Error, "Game not started, players position not set");
                }
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Map {0} doesn't exist", mapId);
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Game already started");
        }

        private void OnChangeDirection(IPlayer player, Directions direction)
        {
            Log.WriteLine(Log.LogLevels.Info, "ChangeDirection {0} to {1}", player.Name, direction);

            if (State == ServerStates.GameStarted && player.State == PlayerStates.Playing)
                // TODO: queue action
                ChangeDirectionAction(player, direction);
        }

        private void OnMove(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "Move {0}", player.Name);

            if (State == ServerStates.GameStarted && player.State == PlayerStates.Playing)
                // TODO: queue action
                MoveAction(player);
        }

        private void OnPlaceBomb(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPlaceBomb {0}", player.Name);

            if (State == ServerStates.GameStarted && player.State == PlayerStates.Playing)
                // TODO: queue action
                PlaceBombAction(player);
        }

        private void OnChat(IPlayer player, string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "Chat from {0}:{1}", player.Name, msg);

            // Send message to other players
            int id = _playerManager.GetId(player);
            foreach (IPlayer other in _playerManager.Players.Where(x => x != player))
                other.OnChatReceived(id, msg);
        }

        //
        private void CheckDeathsAndWinnerOrDraw()
        {
            // Inform dying player and other about dying player
            foreach (IPlayer player in _playerManager.Players.Where(p => p.State == PlayerStates.Dying))
            {
                // Inform player he/she's dead
                player.OnGameLost();

                // Inform other player about player's death
                int playerId = _playerManager.GetId(player);
                IPlayer player1 = player;
                foreach (IPlayer other in _playerManager.Players.Where(x => x != player1))
                    other.OnKilled(playerId);
            }

            //  if no player playing -> draw
            //  if 1 player playing -> winner
            int playingCount = _playerManager.Players.Count(x => x.State == PlayerStates.Playing);
            if (playingCount == 0)
            {
                Log.WriteLine(Log.LogLevels.Info, "Game ended in a DRAW");
                // Inform players about draw
                foreach (IPlayer player in _playerManager.Players)
                    player.OnGameDraw();
                // Change server state
                State = ServerStates.WaitStartGame;
            }
            else if (playingCount == 1 && PlayersInGameCount > 1) // Solo game doesn't stop when only one player left
            {
                IPlayer winner = _playerManager.Players.First(x => x.State == PlayerStates.Playing);
                int id = _playerManager.GetId(winner);

                Log.WriteLine(Log.LogLevels.Info, "Player {0}|{1} WON", winner.Name, id);
                // Inform players about winner
                foreach (IPlayer player in _playerManager.Players)
                    player.OnGameWon(id);
                // Change server state
                State = ServerStates.WaitStartGame;
            }

            // Change dying to died
            foreach (IPlayer p in _playerManager.Players)
                if (p.State == PlayerStates.Dying)
                {
                    int id = _playerManager.GetId(p);
                    Log.WriteLine(Log.LogLevels.Debug, "Kill definitively {0}|{1}", p.Name, id);
                    p.State = PlayerStates.Dead;
                }
        }

        // Actions
        private void ChangeDirectionAction(IPlayer player, Directions direction)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Direction changed successfully -> {0}", direction);

            player.Direction = direction;

            // No need to inform player, ChangeDirection cannot fail
        }

        private void MoveAction(IPlayer player)
        {
            // Get old coordinates
            int oldLocationX = player.LocationX;
            int oldLocationY = player.LocationY;

            // Get new coordinates
            int stepX, stepY;
            GetDirectionSteps(player.Direction, out stepX, out stepY);
            int newLocationX = ComputeLocation(oldLocationX, stepX);
            int newLocationY = ComputeLocation(oldLocationY, stepY);

            // Check if collider on new location
            EntityTypes collider = GameMap.GetEntity(newLocationX, newLocationY);
            if (collider == EntityTypes.Empty) // can only move to empty location
            {
                Log.WriteLine(Log.LogLevels.Debug, "Moved successfully from {0},{1} to {2},{3}", oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Set new location
                player.LocationX = newLocationX;
                player.LocationY = newLocationY;

                // Move player on map
                GameMap.DeleteEntity(oldLocationX, oldLocationY, player.PlayerEntity);
                GameMap.AddEntity(newLocationX, newLocationY, player.PlayerEntity);

                // TODO: get bonus if any

                // Inform player about its new location
                player.OnMoved(true, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Inform other player about player new location
                foreach (IPlayer other in _playerManager.Players.Where(x => x != player))
                    other.OnEntityMoved(player.PlayerEntity, oldLocationX, oldLocationY, newLocationX, newLocationY);
            }
            else if (IsFlame(collider)) // dead
            {
                Log.WriteLine(Log.LogLevels.Debug, "Moved successfully from {0},{1} to {2},{3} but died because of Flames", oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Kill player
                player.State = PlayerStates.Dying;

                // Set new location
                player.LocationX = newLocationX;
                player.LocationY = newLocationY;

                // Move player on map
                GameMap.DeleteEntity(oldLocationX, oldLocationY, player.PlayerEntity);
                GameMap.AddEntity(newLocationX, newLocationY, player.PlayerEntity);

                // Inform player about its new location
                player.OnMoved(true, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Inform other player about player new location
                foreach (IPlayer other in _playerManager.Players.Where(x => x != player))
                    other.OnEntityMoved(player.PlayerEntity, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Check deaths, winner or draw
                CheckDeathsAndWinnerOrDraw();
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Debug, "Moved from {0},{1} to {2},{3} failed because of collider {4}", oldLocationX, oldLocationY, newLocationX, newLocationY, collider);
                player.OnMoved(false, -1, -1, -1, -1);
            }
        }

        private void PlaceBombAction(IPlayer player)
        {
            // TODO: check player max bomb
            // TODO: can't place 2 bombs at the same place or a bonus+bomb
            // TODO: if place a bomb in flames -> immediate explosion

            int stepX, stepY;
            GetDirectionSteps(player.Direction, out stepX, out stepY);
            int bombLocationX = ComputeLocation(player.LocationX, stepX);
            int bombLocationY = ComputeLocation(player.LocationY, stepY);

            EntityTypes collider = GameMap.GetEntity(bombLocationX, bombLocationY);
            if (collider == EntityTypes.Empty) // Cannot place bomb if collider
            {
                // Add bomb to map
                GameMap.AddEntity(bombLocationX, bombLocationY, EntityTypes.Bomb);

                // Add bomb timer to timer map
                int index = GameMap.GetIndex(bombLocationX, bombLocationY);
                _bombTimerTable[index] = BombTimer; // TODO: timer may be modified with bonus (+ range)

                // Inform player about bomb placement
                player.OnBombPlaced(true, EntityTypes.Bomb, bombLocationX, bombLocationY);

                // Inform other player about bomb placement
                foreach (IPlayer other in _playerManager.Players.Where(p => p != player))
                    other.OnEntityAdded(EntityTypes.Bomb, bombLocationX, bombLocationY);

                Log.WriteLine(Log.LogLevels.Debug, "Bomb placed by {0} at {1},{2}: {3}", player.Name, bombLocationX, bombLocationY, EntityTypes.Bomb);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Debug, "Place bomb failed");
                player.OnBombPlaced(false, EntityTypes.Empty, -1, -1);
            }
        }

        private void ExplosionAction(int index)
        {
            // TODO: explosion should be linked to a player to check bonus
            int x, y;
            GameMap.GetLocation(index, out x, out y);

            List<MapModification> modifications = GenerateExplosionModifications(x, y, true, MaxExplosionRange);

            if (modifications.Any())
            {
                foreach (IPlayer player in _playerManager.Players)
                    player.OnMapModified(modifications);
            }

            // Check deaths, winner or draw
            CheckDeathsAndWinnerOrDraw();
        }

        private List<MapModification> GenerateExplosionModifications(int x, int y, bool addFlames, int explosionRange)
        {
            List<MapModification> modifications = new List<MapModification>();

            Log.WriteLine(Log.LogLevels.Debug, "Explosion at {0}, {1}", x, y);

            int index = GameMap.GetIndex(x, y);

            EntityTypes entity = GameMap.GetEntity(x, y);

            // Destroy dust
            if (IsDust(entity))
            {
                Log.WriteLine(Log.LogLevels.Debug, "Dust removed at {0},{1}", x, y);
                AddModification(modifications, x, y, EntityTypes.Dust, MapModificationActions.Delete);
            }

            // Kill player
            if (IsPlayer(entity))
            {
                EntityTypes playerEntity = GetPlayer(entity);
                if (playerEntity != EntityTypes.Empty)
                {
                    IPlayer player = _playerManager.Players.FirstOrDefault(p => p.PlayerEntity == playerEntity);
                    if (player != null)
                    {
                        int id = _playerManager.GetId(player);
                        Log.WriteLine(Log.LogLevels.Info, "Player {0}|{1} dead due to explosion at {2},{3}: {4}", player.Name, id, x, y, entity);
                        // Kill player (will be killed definitively when every explosion modification will be done)
                        player.State = PlayerStates.Dying;
                    }
                    else
                        Log.WriteLine(Log.LogLevels.Error, "Dying player not found in player list at {0},{1}: {2}", x, y, entity);
                }
                else
                    Log.WriteLine(Log.LogLevels.Error, "Dying player without any player at {0},{1}: {2}", x, y, entity);
            }

            if (addFlames)
            {
                // Add flame
                AddModification(modifications, x, y, EntityTypes.Flames, MapModificationActions.Add);
                // Add flame timer
                _flameTimerTable[index] = FlameTimer;

                Log.WriteLine(Log.LogLevels.Debug, "Flame at {0},{1}", x, y);
            }

            if (IsBomb(entity)) // Bomb found, check neighbourhood
            {
                Log.WriteLine(Log.LogLevels.Debug, "Bomb at {0},{1}", x, y);

                // Remove bomb
                Log.WriteLine(Log.LogLevels.Debug, "Remove bomb at {0},{1}", x, y);
                AddModification(modifications, x, y, EntityTypes.Bomb, MapModificationActions.Delete);
                _bombTimerTable[index] = -1; // remove bomb timer

                // Check explosion in surrounding cell and remove cell
                for (int neighbour = 0; neighbour < BombNeighboursX.Length; neighbour++)
                    for (int range = 1; range <= explosionRange; range++)
                    {
                        int stepX = BombNeighboursX[neighbour]*range;
                        int stepY = BombNeighboursY[neighbour]*range;
                        int neighbourX = ComputeLocation(x, stepX);
                        int neighbourY = ComputeLocation(y, stepY);

                        // Stop explosion propagation if wall found
                        EntityTypes neighbourEntity = GameMap.GetEntity(neighbourX, neighbourY);
                        if (IsWall(neighbourEntity))
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "Stop propagating explosion {0},{1} -> {2},{3} wall found", x, y, neighbourX, neighbourY);
                            break;
                        }

                        Log.WriteLine(Log.LogLevels.Debug, "Propagating explosion to neighbourhood {0},{1} -> {2},{3}", x, y, neighbourX, neighbourY);

                        List<MapModification> neighbourModifications = GenerateExplosionModifications(neighbourX, neighbourY, addFlames, range);
                        modifications.AddRange(neighbourModifications);
                    }
                Log.WriteLine(Log.LogLevels.Debug, "Bomb completed at {0},{1}", x, y);
            }

            return modifications;
        }

        private void AddModification(List<MapModification> modifications, int x, int y, EntityTypes entity, MapModificationActions action)
        {
            if (action == MapModificationActions.Add)
                GameMap.AddEntity(x, y, entity);
            else if (action == MapModificationActions.Delete)
                GameMap.DeleteEntity(x, y, entity);
            MapModification modification = new MapModification
                {
                    X = x,
                    Y = y,
                    Entity = entity,
                    Action = action,
                };
            modifications.Add(modification);
        }

        private void FadeBonusAction(int index)
        {
            int x, y;
            GameMap.GetLocation(index, out x, out y);
            EntityTypes entity = GameMap.GetEntity(index);
            EntityTypes bonus = GetBonus(entity);

            if (bonus != EntityTypes.Empty)
            {
                // Delete bonus
                GameMap.DeleteEntity(index, bonus);
                // Remove bonus in every player map
                foreach (IPlayer player in _playerManager.Players)
                    player.OnEntityDeleted(bonus, x, y);
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Faded bonus is not a bonus anymore at {0},{1}: {2}", x, y, entity);
        }

        private void FadeFlameAction(int index)
        {
            int x, y;
            GameMap.GetLocation(index, out x, out y);
            EntityTypes entity = GameMap.GetEntity(index);
            EntityTypes flame = GetFlame(entity);

            if (flame != EntityTypes.Empty)
            {
                // Delete flame
                GameMap.DeleteEntity(index, flame);
                // Remove flame in every player map
                foreach (IPlayer player in _playerManager.Players)
                    player.OnEntityDeleted(flame, x, y);
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Faded flame is not a flame anymore at {0},{1}: {2}", x, y, entity);
        }

        // Tasks
        private void HandleTimerTable(int sleepTime, int[] table, int index, Func<EntityTypes, bool> entityCheckFunc, Action<int> action)
        {
            if (table != null && table[index] > 0)
            {
                int remaining = table[index] - sleepTime;
                if (remaining <= 0)
                {
                    table[index] = -1;

                    EntityTypes entity = GameMap.GetEntity(index);
                    if (entityCheckFunc(entity))
                    {
                        Log.WriteLine(Log.LogLevels.Info, "Timer elapsed for index {0}: {1}", index, entity);

                        action(index);
                    }
                    else
                        Log.WriteLine(Log.LogLevels.Warning, "Timer elapsed but invalid entity found {0}: {1}", index, entity);
                }
                else
                    table[index] = remaining;
            }
        }

        private void TimerTableTask()
        {
            const int sleepTime = 25;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;

                // Check if map timer is elapsed
                if (State == ServerStates.GameStarted && _timerTableSize > 0)
                {
                    for (int i = 0; i < _timerTableSize; i++)
                    {
                        HandleTimerTable(sleepTime, _bombTimerTable, i, IsBomb, ExplosionAction);
                        HandleTimerTable(sleepTime, _flameTimerTable, i, IsFlame, FadeFlameAction);
                        HandleTimerTable(sleepTime, _bonusTimerTable, i, IsBonus, FadeBonusAction);
                    }
                }

                bool signaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                if (signaled)
                    break;
            }
        }

        // Helpers
        private static void GetDirectionSteps(Directions direction, out int stepX, out int stepY)
        {
            stepX = 0;
            stepY = 0;
            switch (direction)
            {
                case Directions.Left:
                    stepX = -1;
                    break;
                case Directions.Right:
                    stepX = +1;
                    break;
                case Directions.Up:
                    stepY = -1;
                    break;
                case Directions.Down:
                    stepY = +1;
                    break;
                default:
                    // NOP
                    break;
            }
        }

        private int ComputeLocation(int coord, int step)
        {
            coord = (coord + step)%GameMap.Description.Size;
            if (coord < 0)
                coord += GameMap.Description.Size;
            return coord;
        }

        private static bool IsWall(EntityTypes entity)
        {
            return (entity & EntityTypes.Wall) == EntityTypes.Wall;
        }

        private static bool IsPlayer(EntityTypes entity)
        {
            return (entity & EntityTypes.Player1) == EntityTypes.Player1
                   || (entity & EntityTypes.Player2) == EntityTypes.Player2
                   || (entity & EntityTypes.Player3) == EntityTypes.Player3
                   || (entity & EntityTypes.Player4) == EntityTypes.Player4;
        }

        private static bool IsBomb(EntityTypes entity)
        {
            return (entity & EntityTypes.Bomb) == EntityTypes.Bomb;
        }

        private static bool IsBonus(EntityTypes entity)
        {
            return (entity & EntityTypes.BonusA) == EntityTypes.BonusA
                   || (entity & EntityTypes.BonusB) == EntityTypes.BonusB
                   || (entity & EntityTypes.BonusC) == EntityTypes.BonusC
                   || (entity & EntityTypes.BonusD) == EntityTypes.BonusD
                   || (entity & EntityTypes.BonusE) == EntityTypes.BonusE
                   || (entity & EntityTypes.BonusF) == EntityTypes.BonusF
                   || (entity & EntityTypes.BonusG) == EntityTypes.BonusG
                   || (entity & EntityTypes.BonusH) == EntityTypes.BonusH
                   || (entity & EntityTypes.BonusI) == EntityTypes.BonusI
                   || (entity & EntityTypes.BonusJ) == EntityTypes.BonusJ;
        }

        private static bool IsFlame(EntityTypes entity)
        {
            return (entity & EntityTypes.Flames) == EntityTypes.Flames;
        }

        private static bool IsDust(EntityTypes entity)
        {
            return (entity & EntityTypes.Dust) == EntityTypes.Dust;
        }

        private static EntityTypes GetPlayer(EntityTypes entity)
        {
            if ((entity & EntityTypes.Player1) == EntityTypes.Player1)
                return EntityTypes.Player1;
            if ((entity & EntityTypes.Player2) == EntityTypes.Player2)
                return EntityTypes.Player2;
            if ((entity & EntityTypes.Player3) == EntityTypes.Player3)
                return EntityTypes.Player3;
            if ((entity & EntityTypes.Player4) == EntityTypes.Player4)
                return EntityTypes.Player4;
            return EntityTypes.Empty;
        }

        private static EntityTypes GetBonus(EntityTypes entity)
        {
            if ((entity & EntityTypes.BonusA) == EntityTypes.BonusA)
                return EntityTypes.BonusA;
            if ((entity & EntityTypes.BonusB) == EntityTypes.BonusB)
                return EntityTypes.BonusB;
            if ((entity & EntityTypes.BonusC) == EntityTypes.BonusC)
                return EntityTypes.BonusC;
            if ((entity & EntityTypes.BonusD) == EntityTypes.BonusD)
                return EntityTypes.BonusD;
            if ((entity & EntityTypes.BonusE) == EntityTypes.BonusE)
                return EntityTypes.BonusE;
            if ((entity & EntityTypes.BonusF) == EntityTypes.BonusF)
                return EntityTypes.BonusF;
            if ((entity & EntityTypes.BonusG) == EntityTypes.BonusG)
                return EntityTypes.BonusG;
            if ((entity & EntityTypes.BonusH) == EntityTypes.BonusH)
                return EntityTypes.BonusH;
            if ((entity & EntityTypes.BonusI) == EntityTypes.BonusI)
                return EntityTypes.BonusI;
            if ((entity & EntityTypes.BonusJ) == EntityTypes.BonusJ)
                return EntityTypes.BonusJ;
            return EntityTypes.Empty;
        }

        private static EntityTypes GetFlame(EntityTypes entity)
        {
            if ((entity & EntityTypes.Flames) == EntityTypes.Flames)
                return EntityTypes.Flames;
            return EntityTypes.Empty;
        }
    }

    public class Entity
    {
        public EntityTypes Type { get; set; }
        public int X { get; set; } // coordinates are stored to speed-up cell search from entity
        public int Y { get; set; }

        public Entity(EntityTypes type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;
        }
    }

    public class BombEntity : Entity
    {
        public BombEntity(int x, int y, int range, int delayInMs)
            : base(EntityTypes.Bomb, x, y)
        {
            Range = range;
            ExplosionRemainingTimeout = DateTime.Now.AddMilliseconds(delayInMs);
        }

        public void Move(Directions direction, int delayInMs)
        {
            NextMoveRemainingTimeout = DateTime.Now.AddMilliseconds(delayInMs);
            if (!IsMoving) // Cannot change direction if already moving
            {
                Direction = direction;
                IsMoving = true;
            }
        }

        public int Range { get; set; }
        public DateTime ExplosionRemainingTimeout { get; set; } // in ms

        public bool IsMoving { get; set; }
        public Directions Direction { get; set; }
        public DateTime NextMoveRemainingTimeout { get; set; } // in ms
    }

    public class BonusEntity : Entity
    {
        public BonusEntity(EntityTypes type, int x, int y, int delayInMs)
            : base(type, x, y)
        {
            FadeoutRemainingTimeout = DateTime.Now.AddMilliseconds(delayInMs);
        }

        public DateTime FadeoutRemainingTimeout { get; set; } // in ms
    }

    public class FlameEntity : Entity
    {
        public FlameEntity(int x, int y, int delayInMs) : base(EntityTypes.Flames, x, y)
        {
            FadeoutRemainingTimeout = DateTime.Now.AddMilliseconds(delayInMs);
        }

        public DateTime FadeoutRemainingTimeout { get; set; } // in ms
    }

    public class Cell : List<Entity>
    {
        public Entity GetEntity(EntityTypes type)
        {
            return this.FirstOrDefault(x => x.Type == type);
        }
    }

    public class CellMap
    {
        private readonly Cell[,] _cells;

        public Map OriginalMap { get; private set; }
        public int Size { get; private set; }

        public CellMap(Map map)
        {
            OriginalMap = map;
            Size = map.Description.Size;
            _cells = new Cell[Size, Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++ )
                    _cells[x, y] = new Cell
                    {
                        new Entity(map.GetEntity(x,y), x, y)
                    };
        }

        public Cell GetCell(int x, int y)
        {
            return _cells[x, y];
        }
        
        public Entity GetEntity(EntityTypes type, int x, int y)
        {
            return _cells[x, y].GetEntity(type);
        }

        public void AddEntity(Entity entity)
        {
            Cell cell = _cells[entity.X, entity.Y];
            cell.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            Cell cell = _cells[entity.X, entity.Y];
            cell.Remove(entity);
        }

        public void MoveEntity(Entity entity, int toX, int toY)
        {
            Cell fromCell = _cells[entity.X, entity.Y];
            bool entityFound = fromCell.Any(x => x == entity);
            if (entityFound)
            {
                Cell toCell = _cells[toX, toY];
                fromCell.Remove(entity);
                toCell.Add(entity);
                entity.X = toX;
                entity.Y = toY;
            }
            else
                Log.WriteLine(Log.LogLevels.Error, "Cell at {0},{1} doesn't contain {2}", entity.X, entity.Y, entity.Type);
        }
    }

    public class TimedCellActionList : PriorityQueue<DateTime, Entity>
    {
        public Entity DequeueReadyEntity()
        {
            DateTime timeout = Peek();
            if (timeout < DateTime.Now)
                return Dequeue();
            return null;
        }
    }
}
