using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bomberman.Common;
using Bomberman.Common.DataContracts;
using Bomberman.Common.Randomizer;
using Bomberman.Server.Entities;
using Bomberman.Server.Interfaces;

namespace Bomberman.Server
{
    public class Server : IDisposable
    {
        private static readonly int[] BombNeighboursX = {-1, 1, 0, 0};
        private static readonly int[] BombNeighboursY = {0, 0, -1, 1};

        private readonly IHost _host;
        private readonly IPlayerManager _playerManager;
        private readonly IMapManager _mapManager;
        private readonly IEntityMap _entityMap;
        private readonly List<IOccurancy<EntityTypes>> _bonusOccurancies;

        private readonly BlockingCollection<Action> _gameActionBlockingCollection = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
        private readonly SortedLinkedList<DateTime, Tuple<Entity, Action<Entity>>> _timeoutActionQueue = new SortedLinkedList<DateTime, Tuple<Entity, Action<Entity>>>();

        private CancellationTokenSource _cancellationTokenSource;
        private Task _actionTask;
        private Task _timeoutActionTask;
        private Task _keepAliveTask;
        private ServerStates _state;
        private int _playersInGameCount;

        public Server(IHost host, IPlayerManager playerManager, IMapManager mapManager, IEntityMap entityMap, List<IOccurancy<EntityTypes>> bonusOccurancies)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");
            if (mapManager == null)
                throw new ArgumentNullException("mapManager");
            if (entityMap == null)
                throw new ArgumentNullException("entityMap");
            if (bonusOccurancies == null)
                throw new ArgumentNullException("bonusOccurancies");

            _host = host;
            _playerManager = playerManager;
            _mapManager = mapManager;
            _entityMap = entityMap;
            _bonusOccurancies = bonusOccurancies;

            _host.HostLogin += OnHostLogin;
            _host.HostLogout += OnHostLogout;
            _host.HostStartGame += OnHostStartGame;
            _host.HostMove += OnHostMove;
            _host.HostPlaceBomb += OnHostPlaceBomb;
            _host.HostChat += OnHostChat;

            _host.PlayerDisconnected += OnPlayerDisconnected;

            _state = ServerStates.WaitStartGame;
        }
        
        public void Start()
        {
            _host.Start();

            _cancellationTokenSource = new CancellationTokenSource();
            _actionTask = Task.Factory.StartNew(GameActionTask, _cancellationTokenSource.Token);
            _timeoutActionTask = Task.Factory.StartNew(TimeoutGameActionTask, _cancellationTokenSource.Token);
            _keepAliveTask = Task.Factory.StartNew(KeepAliveTask, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _host.Stop();

            _cancellationTokenSource.Cancel();
            Task.WaitAll(new[] { _actionTask, _timeoutActionTask, _keepAliveTask }, 1000);
        }

        // IHost handlers
        private void OnHostLogin(IPlayer player, int playerId)
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
            player.OnLogin(LoginResults.Successful, playerId, playerEntity, _mapManager.MapDescriptions, _state == ServerStates.GameStarted);

            // Inform player about other players
            foreach (IPlayer other in _playerManager.Players.Where(x => x != player))
            {
                int otherId = _playerManager.GetId(other);
                player.OnUserConnected(other.Name, otherId);
            }

            // Inform other players about new player
            foreach (IPlayer other in _playerManager.Players.Where(x => x != player))
                other.OnUserConnected(player.Name, playerId);
        }

        private void OnHostLogout(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "Player {0} logs out", player.Name);
            RemovePlayerGracefully(player);
        }

        private void OnHostStartGame(IPlayer player, int mapId)
        {
            Log.WriteLine(Log.LogLevels.Info, "Start game {0} map {1}", player.Name, mapId);

            if (_state == ServerStates.WaitStartGame)
            {
                ResetActionQueues();

                // Generate cell map
                Map map = _mapManager.Maps.FirstOrDefault(x => x.Description.Id == mapId);
                if (map != null)
                {
                    // Clone map because unused player slot will be replaced with dust
                    Map clonedMap = new Map
                        {
                            Description = map.Description,
                            MapAsArray = (EntityTypes[]) map.MapAsArray.Clone()
                        };

                    bool positionError = false;
                    // Set players position
                    foreach (IPlayer p in _playerManager.Players)
                    {
                        var entry = clonedMap.MapAsArray.Select((entity, index) => new
                            {
                                entity,
                                index
                            }).FirstOrDefault(x => x.entity == p.PlayerEntity);
                        if (entry != null)
                        {
                            // Set player position
                            int x, y;
                            clonedMap.GetLocation(entry.index, out x, out y);
                            p.LocationX = x;
                            p.LocationY = y;
                        }
                        else
                        {
                            Log.WriteLine(Log.LogLevels.Error, "Cannot find position of {0} in map {1}", player.PlayerEntity, clonedMap.Description.Id);
                            positionError = true;
                            break;
                        }
                    }

                    // Replace unused player with dust (this is the reason why we cloned the map)
                    for (int i = 0; i < clonedMap.MapAsArray.Length; i++)
                    {
                        EntityTypes entity = clonedMap.MapAsArray[i];
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
                                    clonedMap.MapAsArray[i] = EntityTypes.Dust;
                                }
                            }
                            else
                                Log.WriteLine(Log.LogLevels.Warning, "Inconsistant player position at index {0} in map {1}", i, clonedMap.Description.Id);
                        }
                    }

                    // Initialize entity map
                    _entityMap.Initialize(clonedMap);

                    // If no problem while positioning player, let's go
                    if (!positionError)
                    {
                        // Reset player
                        foreach (IPlayer p in _playerManager.Players)
                        {
                            p.State = PlayerStates.Playing;
                            p.BombCount = 0;
                            p.MaxBombCount = ServerOptions.MinBomb;
                            p.BombRange = ServerOptions.MinExplosionRange;
                            p.Bonuses = new List<EntityTypes>();

                            //p.Bonuses.Add(EntityTypes.BonusBombKick); // TODO: remove
                        }

                        //
                        _playersInGameCount = _playerManager.Players.Count(x => x.State == PlayerStates.Playing);

                        // Inform players about game started
                        foreach (IPlayer p in _playerManager.Players)
                            p.OnGameStarted(p.LocationX, p.LocationY, clonedMap);

                        _state = ServerStates.GameStarted;
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

        private void OnHostMove(IPlayer player, Directions direction)
        {
            Log.WriteLine(Log.LogLevels.Info, "Move {0}:{1}", player.Name, direction);

            if (_state == ServerStates.GameStarted && player.State == PlayerStates.Playing)
                EnqueueGameAction(() => MovePlayerAction(player, direction));
        }

        private void OnHostPlaceBomb(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnHostPlaceBomb {0}", player.Name);

            if (_state == ServerStates.GameStarted && player.State == PlayerStates.Playing)
                EnqueueGameAction(() => PlaceBombAction(player));
        }

        private void OnHostChat(IPlayer player, string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "Chat from {0}:{1}", player.Name, msg);

            // Send message to other players
            int id = _playerManager.GetId(player);
            foreach (IPlayer other in _playerManager.Players.Where(x => x != player))
                other.OnChatReceived(id, msg);
        }

        private void OnPlayerDisconnected(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "Player {0} disconnected", player.Name);
            RemovePlayerGracefully(player);
        }

        //
        private void ResetActionQueues()
        {
            // Reset action list
            while (_gameActionBlockingCollection.Count > 0)
            {
                Action item;
                _gameActionBlockingCollection.TryTake(out item);
            }

            // Reset timeout action queue
            lock (_timeoutActionQueue)
                _timeoutActionQueue.Clear();
        }

        private void RemovePlayerGracefully(IPlayer player)
        {
            int id;
            // Remove player from player list
            lock (_playerManager.LockObject)
            {
                // Get id
                id = _playerManager.GetId(player);
                // Remove player from player list
                _playerManager.Remove(player);
            }

            // Check winner or draw
            if (_state == ServerStates.GameStarted)
                CheckDeathsAndWinnerOrDraw();

            // Inform other players
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnUserDisconnected(id);
        }

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
                foreach (IPlayer other in _playerManager.Players.Where(p => p != player1))
                    other.OnKilled(playerId, player.PlayerEntity, player.LocationX, player.LocationY);
            }

            //  if no player playing
            //      if only one player in game -> lost
            //      else -> draw
            //  if 1 player playing -> winner
            int playingCount = _playerManager.Players.Count(p => p.State == PlayerStates.Playing);
            if (playingCount == 0)
            {
                if (_playersInGameCount > 1)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Game ended in a DRAW");
                    // Inform players about draw
                    foreach (IPlayer player in _playerManager.Players)
                        player.OnGameDraw();
                    //
                    ResetActionQueues();
                    // Change server state
                    _state = ServerStates.WaitStartGame;
                }
                else
                {
                    Log.WriteLine(Log.LogLevels.Info, "Solo game ended");
                    //
                    ResetActionQueues();
                    // Change server state
                    _state = ServerStates.WaitStartGame;
                }
            }
            else if (playingCount == 1 && _playersInGameCount > 1) // Solo game doesn't stop when only one player left
            {
                IPlayer winner = _playerManager.Players.First(x => x.State == PlayerStates.Playing);
                int id = _playerManager.GetId(winner);

                Log.WriteLine(Log.LogLevels.Info, "Player {0}|{1} WON", winner.Name, id);
                // Inform players about winner
                foreach (IPlayer player in _playerManager.Players)
                    player.OnGameWon(id);
                //
                ResetActionQueues();
                // Change server state
                _state = ServerStates.WaitStartGame;
            }

            // Change dying to died
            foreach (IPlayer player in _playerManager.Players.Where(p => p.State == PlayerStates.Dying))
            {
                int id = _playerManager.GetId(player);
                Log.WriteLine(Log.LogLevels.Debug, "Kill definitively {0}|{1}", player.Name, id);
                player.State = PlayerStates.Dead;
            }
        }

        // Actions

        private void MovePlayerAction(IPlayer player, Directions direction)
        {
            // Get old coordinates
            int oldLocationX = player.LocationX;
            int oldLocationY = player.LocationY;

            // Get current location cell
            IEntityCell currentCell = _entityMap.GetCell(oldLocationX, oldLocationY);

            // Search player in cell map
            //IEntity playerEntity = _entityMap.GetEntity(player.PlayerEntity, oldLocationX, oldLocationY);
            IEntity playerEntity = currentCell.Entities.FirstOrDefault(x => x.Type == player.PlayerEntity);

            // Get new coordinates
            int newLocationX, newLocationY;
            _entityMap.ComputeNewCoordinates(playerEntity, direction, out newLocationX, out newLocationY);

            // Get collider on new location
            IEntityCell colliderCell = _entityMap.GetCell(newLocationX, newLocationY);

            //
            if ((ServerOptions.BombKickBehaviour == BombKickBehaviours.OnAnyCell || ServerOptions.BombKickBehaviour == BombKickBehaviours.OnPlayerCellOnly)
                && player.Bonuses.Any(b => b == EntityTypes.BonusBombKick) && currentCell.Entities.Any(e => e.IsBomb) && colliderCell.Entities.All(e => e.IsEmpty || e.IsBonus)) // Can kick bomb if bonus
            {
                Log.WriteLine(Log.LogLevels.Debug, "Push bomb at {0},{1}", newLocationX, newLocationY);

                BombEntity bomb = currentCell.GetEntity(EntityTypes.Bomb) as BombEntity;
                if (bomb != null)
                {
                    if (!bomb.IsMoving)
                    {
                        // Init move
                        bomb.InitMove(direction, TimeSpan.FromMilliseconds(ServerOptions.BombMoveTimer));
                        // Perform move action immediately
                        MoveBombAction(bomb);
                    }
                    else
                        Log.WriteLine(Log.LogLevels.Debug, "Cannot push bomb because its already moving");
                }
                else
                    Log.WriteLine(Log.LogLevels.Error, "Pushed bomb doesn't exist anymore at this location {0},{1}", newLocationX, newLocationY);
            }
            else if (player.Bonuses.Any(b => b == EntityTypes.BonusNoClipping) || colliderCell.Entities.All(e => e.IsEmpty || e.IsBonus)) // can only move to empty location or bonus, or everywhere if no-clip bonus
            {
                Log.WriteLine(Log.LogLevels.Debug, "Moved successfully from {0},{1} to {2},{3}", oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Get bonus if any
                EntityTypes bonusEntity = EntityTypes.Empty;
                BonusEntity bonus = colliderCell.Entities.FirstOrDefault(x => x.IsBonus) as BonusEntity;
                if (bonus != null)
                {
                    bonusEntity = bonus.Type;

                    if (bonusEntity == EntityTypes.BonusBombUp)
                        player.MaxBombCount = Math.Min(player.MaxBombCount + 1, ServerOptions.MaxBomb);
                    else if (bonusEntity == EntityTypes.BonusBombDown)
                        player.MaxBombCount = Math.Max(player.MaxBombCount - 1, ServerOptions.MinBomb);
                    else if (bonusEntity == EntityTypes.BonusFireUp)
                        player.BombRange = Math.Min(player.BombRange + 1, ServerOptions.MaxExplosionRange);
                    else if (bonusEntity == EntityTypes.BonusFireDown)
                        player.BombRange = Math.Max(player.BombRange - 1, ServerOptions.MinExplosionRange);
                    else // TODO: Don't add twice the same bonus
                        player.Bonuses.Add(bonusEntity); // state bonus

                    Log.WriteLine(Log.LogLevels.Info, "Player {0} picked up bonus {1}", player.Name, bonusEntity);
                }

                // Set new location
                player.LocationX = newLocationX;
                player.LocationY = newLocationY;

                // Inform player about its new location
                player.OnMoved(true, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Remove bonus from map and inform player about bonus picked up if any
                if (bonusEntity != EntityTypes.Empty)
                {
                    _entityMap.RemoveEntity(bonus);
                    lock (_timeoutActionQueue)
                        _timeoutActionQueue.RemoveAll(e => e.Item1 == bonus);
                    player.OnBonusPickedUp(bonusEntity, newLocationX, newLocationY);
                }

                // Move player on map
                _entityMap.MoveEntity(playerEntity, newLocationX, newLocationY);

                // Inform other players about player move and bonus
                foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                {
                    p.OnEntityMoved(player.PlayerEntity, oldLocationX, oldLocationY, newLocationX, newLocationY);
                    p.OnEntityDeleted(bonusEntity, newLocationX, newLocationY);
                }
            }
            else if ((ServerOptions.BombKickBehaviour == BombKickBehaviours.OnAnyCell || ServerOptions.BombKickBehaviour == BombKickBehaviours.OnDestinationCellOnly)
                && player.Bonuses.Any(b => b == EntityTypes.BonusBombKick) && colliderCell.Entities.Any(e => e.IsBomb)) // Can kick bomb if bonus gained
            {
                Log.WriteLine(Log.LogLevels.Debug, "Push bomb at {0},{1}", newLocationX, newLocationY);

                BombEntity bomb = colliderCell.GetEntity(EntityTypes.Bomb) as BombEntity;
                if (bomb != null)
                {
                    if (!bomb.IsMoving)
                    {
                        // Init move
                        bomb.InitMove(direction, TimeSpan.FromMilliseconds(ServerOptions.BombMoveTimer));
                        // Perform move action immediately
                        MoveBombAction(bomb);
                    }
                    else
                        Log.WriteLine(Log.LogLevels.Debug, "Cannot push bomb because its already moving");
                }
                else
                    Log.WriteLine(Log.LogLevels.Error, "Pushed bomb doesn't exist anymore at this location {0},{1}", newLocationX, newLocationY);
            }
            else if (colliderCell.Entities.Any(e => e.IsFlames)) // dead
            {
                Log.WriteLine(Log.LogLevels.Debug, "Moved successfully from {0},{1} to {2},{3} but died because of Flames", oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Kill player
                player.State = PlayerStates.Dying;

                // Set new location
                player.LocationX = newLocationX;
                player.LocationY = newLocationY;

                // Inform player about its new location
                player.OnMoved(true, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Move player on map
                _entityMap.MoveEntity(playerEntity, newLocationX, newLocationY);

                // Inform other players about player move
                foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                    p.OnEntityMoved(player.PlayerEntity, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Check deaths, winner or draw
                CheckDeathsAndWinnerOrDraw();
            }
            else
            {
                string collider = colliderCell.Entities.Select(x => x.Type.ToString()).Aggregate((s, s1) => s + s1);
                Log.WriteLine(Log.LogLevels.Debug, "Moved from {0},{1} to {2},{3} failed because of collider {4}", oldLocationX, oldLocationY, newLocationX, newLocationY, collider);
                player.OnMoved(false, -1, -1, -1, -1);
            }
        }

        private void PlaceBombAction(IPlayer player)
        {
            // Check max player bomb
            if (player.BombCount >= player.MaxBombCount)
            {
                // Inform player about bomb fail
                player.OnBombPlaced(PlaceBombResults.TooManyBombs, EntityTypes.Empty, -1, -1);

                Log.WriteLine(Log.LogLevels.Debug, "Bomb not placed by {0} at {1},{2}: too many bomb", player.Name, player.LocationX, player.LocationY);
            }
            else
            {
                int bombLocationX = player.LocationX;
                int bombLocationY = player.LocationY;
                int bombExplosionRange = player.BombRange;

                // Create bomb
                BombEntity bomb = new BombEntity(player, bombLocationX, bombLocationY, bombExplosionRange, TimeSpan.FromMilliseconds(ServerOptions.BombTimer));

                // Increase bomb count
                player.BombCount++;

                // Inform player about bomb placement
                player.OnBombPlaced(PlaceBombResults.Successful, EntityTypes.Bomb, bombLocationX, bombLocationY);

                // Add bomb to map
                _entityMap.AddEntity(bomb);
                EnqueueTimeoutGameAction(DateTime.Now.AddMilliseconds(ServerOptions.BombTimer), bomb, e => ExplosionAction(e as BombEntity));

                // Inform other players about bomb placement
                foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                    p.OnEntityAdded(EntityTypes.Bomb, bombLocationX, bombLocationY);

                Log.WriteLine(Log.LogLevels.Debug, "Bomb placed by {0} at {1},{2}: {3}", player.Name, bombLocationX, bombLocationY, EntityTypes.Bomb);
            }
        }

        private void MoveBombAction(BombEntity bomb)
        {
            //  If collider is Flame (other collider only forbids move)
            //      Move
            //      Inform players about move
            //      Explosion
            //  Else, (no collider)
            //      Move
            //      Inform players about move
            //      Reset move timer
            //      Insert bomb in timed action queue

            int fromX = bomb.X;
            int fromY = bomb.Y;
            IEntityCell cell = _entityMap.GetCell(fromX, fromY);
            if (cell.Entities.Any(e => e == bomb))
            {
                // Try to move bomb
                int toX, toY;
                _entityMap.ComputeNewCoordinates(bomb, bomb.Direction, out toX, out toY);
                // Check collider
                IEntityCell destinationCell = _entityMap.GetCell(toX, toY);
                if (destinationCell.Entities.Any(x => x.IsFlames)) // collision with flame -> explosion
                {
                    Log.WriteLine(Log.LogLevels.Debug, "Move bomb collision with flames -> explosion");

                    // Move
                    _entityMap.MoveEntity(bomb, toX, toY);

                    // Inform players about bomb move
                    foreach (IPlayer player in _playerManager.Players)
                        player.OnEntityMoved(bomb.Type, fromX, fromY, toX, toY);

                    // Explosion
                    ExplosionAction(bomb);
                }
                else if (destinationCell.Entities.All(x => x.IsEmpty || x.IsBonus))
                {
                    Log.WriteLine(Log.LogLevels.Debug, "Move bomb no collision");

                    // Move
                    _entityMap.MoveEntity(bomb, toX, toY);

                    // Reset move timer
                    bomb.InitMove(bomb.Direction, bomb.MoveDelay);
                    // Insert bomb in timed action queue
                    EnqueueTimeoutGameAction(bomb.MoveTimeout, bomb, e => MoveBombAction(e as BombEntity));

                    // Inform players about bomb move
                    foreach (IPlayer player in _playerManager.Players)
                        player.OnEntityMoved(bomb.Type, fromX, fromY, toX, toY);
                }
                else
                {
                    string colliderList = destinationCell.Entities.Any() ? destinationCell.Entities.Select(e => e.Type.ToString()).Aggregate((s, s1) => s + s1) : "???";
                    Log.WriteLine(Log.LogLevels.Debug, "Move bomb collision with {0} -> stopped", colliderList);
                }
            }
            else
                Log.WriteLine(Log.LogLevels.Debug, "MoveBombAction on inexistant bomb at {0},{1}", bomb.X, bomb.Y);
        }

        private void FadeOutFlameAction(FlameEntity flame)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Flame fades out at {0},{1}", flame.X, flame.Y);
            // Remove flame from map
            _entityMap.RemoveEntity(flame); // no need to remove from timeout action
            // Inform players about flame fadout
            foreach (IPlayer player in _playerManager.Players)
                player.OnEntityDeleted(flame.Type, flame.X, flame.Y);
        }

        private void FadeOutBonusAction(BonusEntity bonus)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Bonus fades out at {0},{1}", bonus.X, bonus.Y);
            // Remove bonus from map
            _entityMap.RemoveEntity(bonus); // no need to remove from timeout action
            // Inform players about bonus fadout
            foreach (IPlayer player in _playerManager.Players)
                player.OnEntityDeleted(bonus.Type, bonus.X, bonus.Y);
        }

        private void ExplosionAction(BombEntity bomb)
        {
            IEntityCell cell = _entityMap.GetCell(bomb.X, bomb.Y);
            if (cell.Entities.Any(e => e == bomb))
            {
                bool addFlames = bomb.Player.Bonuses.Any(x => x == EntityTypes.BonusFlameBomb);
                List<MapModification> modifications = new List<MapModification>();
                GenerateExplosionModifications(bomb.X, bomb.Y, addFlames, bomb.Range, modifications);

                if (modifications.Any())
                {
                    foreach (IPlayer player in _playerManager.Players)
                        player.OnEntitiesModified(modifications);
                }

                CheckDeathsAndWinnerOrDraw();
            }
            else
                Log.WriteLine(Log.LogLevels.Debug, "ExplosionAction on inexistant bomb at {0},{1}", bomb.X, bomb.Y);
        }

        private void AddMapModification(List<MapModification> list, IEntity entity, MapModificationOperations operation)
        {
            switch (operation)
            {
                case MapModificationOperations.Add:
                    _entityMap.AddEntity(entity); // Modification will notified to player later
                    break;
                case MapModificationOperations.Delete:
                    _entityMap.RemoveEntity(entity); // Modification will notified to player later
                    break;
                case MapModificationOperations.Explosion:
                    // NOP
                    break;
            }
            MapModification modification = new MapModification
            {
                X = entity.X,
                Y = entity.Y,
                Entity = entity.Type,
                Operation = operation
            };
            list.Add(modification);
        }

        private void AddExplosionModification(List<MapModification> list, int x, int y)
        {
            MapModification modification = new MapModification
            {
                X = x,
                Y = y,
                Entity = EntityTypes.Empty,
                Operation = MapModificationOperations.Explosion
            };
            list.Add(modification);
        }

        private bool GenerateExplosionModifications(int x, int y, bool addFlames, int explosionRange, List<MapModification> modifications)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Explosion at {0}, {1}", x, y);

            IEntityCell cell = _entityMap.GetCell(x, y);

            if (cell.Entities.Any(e => e.IsWall))
            {
                Log.WriteLine(Log.LogLevels.Debug, "Wall found at explosion location {0},{1} -> don't do anything", x, y);
                return true;
            }

            bool obstacleFound = false;

            // Cell is affected by explosion
            AddExplosionModification(modifications, x, y);

            // Destroy dust and generate bonus
            if (cell.Entities.Any(e => e.IsDust))
            {
                IEntity dust = cell.Entities.FirstOrDefault(e => e.IsDust);
                if (dust != null)
                {
                    Log.WriteLine(Log.LogLevels.Debug, "Dust removed at {0},{1}", x, y);
                    AddMapModification(modifications, dust, MapModificationOperations.Delete);

                    // Get random bonus
                    EntityTypes bonusType = RangeRandom.Random(_bonusOccurancies);
                    if (bonusType != EntityTypes.Empty)
                    {
                        // Add bonus
                        BonusEntity bonus = new BonusEntity(bonusType, x, y, TimeSpan.FromMilliseconds(ServerOptions.BonusTimer));
                        AddMapModification(modifications, bonus, MapModificationOperations.Add);
                        EnqueueTimeoutGameAction(bonus.FadeoutTimeout, bonus, e => FadeOutBonusAction(e as BonusEntity));
                    }
                }
                obstacleFound = true;
            }

            // Kill player
            if (cell.Entities.Any(e => e.IsPlayer))
            {
                List<Entity> toRemove = new List<Entity>();
                foreach (Entity entity in cell.Entities.Where(e => e.IsPlayer))
                {
                    IPlayer player = _playerManager.Players.FirstOrDefault(p => p.PlayerEntity == entity.Type);
                    if (player != null)
                    {
                        Log.WriteLine(Log.LogLevels.Info, "Player {0} dead due to explosion at {1},{2}: {3}", player.Name, x, y, entity.Type);
                        // Kill player (will be killed definitively when every explosion modifications will be handled)
                        player.State = PlayerStates.Dying;
                    }
                    else
                        Log.WriteLine(Log.LogLevels.Error, "Dying player not found in player list at {0},{1}: {2}", x, y, entity.Type);
                    toRemove.Add(entity);
                }
                cell.Entities.RemoveAll(toRemove.Contains);
                obstacleFound = true;
            }

            if (addFlames) // TODO: if flame already exists, update flame timeout
            {
                if (cell.Entities.All(e => !e.IsFlames))
                {
                    // Create flame
                    FlameEntity flame = new FlameEntity(x, y, TimeSpan.FromMilliseconds(ServerOptions.FlameTimer));
                    AddMapModification(modifications, flame, MapModificationOperations.Add);
                    EnqueueTimeoutGameAction(flame.FadeoutTimeout, flame, e => FadeOutFlameAction(e as FlameEntity));

                    Log.WriteLine(Log.LogLevels.Debug, "Flame at {0},{1}", x, y);
                }
                else
                    Log.WriteLine(Log.LogLevels.Debug, "Duplicate flame at {0},{1}", x, y);
            }

            if (cell.Entities.Any(e => e.IsBomb)) // Bomb found, check neighbourhood
            {
                IEntity bombEntity = cell.Entities.FirstOrDefault(e => e.IsBomb);
                //foreach (Entity bombEntity in cell.Where(e => e.IsBomb))
                if (bombEntity != null)
                {
                    Log.WriteLine(Log.LogLevels.Debug, "Bomb at {0},{1}", x, y);

                    // Remove bomb
                    Log.WriteLine(Log.LogLevels.Debug, "Remove bomb at {0},{1}", x, y);
                    AddMapModification(modifications, bombEntity, MapModificationOperations.Delete);
                    lock(_timeoutActionQueue)
                        _timeoutActionQueue.RemoveAll(e => e.Item1 == bombEntity);

                    // Decrease bomb count
                    BombEntity bomb = bombEntity as BombEntity;
                    if (bomb != null)
                        bomb.Player.BombCount--;

                    // Check explosion in surrounding cell and remove cell
                    for (int neighbourIndex = 0; neighbourIndex < BombNeighboursX.Length; neighbourIndex++)
                        for (int range = 1; range <= explosionRange; range++)
                        {
                            int stepX = BombNeighboursX[neighbourIndex] * range;
                            int stepY = BombNeighboursY[neighbourIndex] * range;
                            int neighbourX = _entityMap.ComputeLocation(x, stepX);
                            int neighbourY = _entityMap.ComputeLocation(y, stepY);

                            // Stop explosion propagation if wall found
                            IEntityCell neighbourCell = _entityMap.GetCell(neighbourX, neighbourY);
                            if (neighbourCell.Entities.Any(e => e.IsWall))
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "Stop propagating explosion {0},{1} -> {2},{3} wall found", x, y, neighbourX, neighbourY);
                                break;
                            }

                            Log.WriteLine(Log.LogLevels.Debug, "Propagating explosion to neighbourhood {0},{1} -> {2},{3}", x, y, neighbourX, neighbourY);

                            bool found = GenerateExplosionModifications(neighbourX, neighbourY, addFlames, range, modifications);
                            // Stop explosion if any obstacle found
                            if (found)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "Stop propagating explosion {0},{1} -> {2},{3} obstacle found while propagating", x, y, neighbourX, neighbourY);
                                break;
                            }
                        }
                    Log.WriteLine(Log.LogLevels.Debug, "Bomb completed at {0},{1}", x, y);
                }
                obstacleFound = true;
            }

            return obstacleFound;
        }

        private void EnqueueGameAction(Action action)
        {
            _gameActionBlockingCollection.Add(action); // TODO: TryAdd ???
        }

        private void EnqueueTimeoutGameAction(DateTime timeout, Entity entity, Action<Entity> action)
        {
            lock (_timeoutActionQueue)
            {
                Log.WriteLine(Log.LogLevels.Debug, "Enqueue timeout action {0:HH:mm:ss.ffffff}", timeout);

                _timeoutActionQueue.Enqueue(timeout, new Tuple<Entity, Action<Entity>>(entity, action));
            }
        }

        private void GameActionTask()
        {
            while (true)
            {
                try
                {
                    Action action;
                    bool taken = _gameActionBlockingCollection.TryTake(out action, 10, _cancellationTokenSource.Token);
                    if (taken)
                    {
                        try
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "Dequeue action, #item in queue {0}", _gameActionBlockingCollection.Count);
                            action();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(Log.LogLevels.Error, "Exception raised in GameActionsTask. Exception:{0}", ex);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Taking cancelled");
                    break;
                }
            }
        }

        private void TimeoutGameActionTask()
        {
            const int sleepTime = 25;
            while (true)
            {
                //Log.WriteLine(Log.LogLevels.Debug, "TimeoutGameActionTask loop  count:{0}", _timeoutActionQueue.Count);
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;

                // Check if map timer is elapsed
                while (true)
                {
                    Tuple<Entity, Action<Entity>> tuple = null;
                    lock (_timeoutActionQueue)
                    {
                        if (_timeoutActionQueue.Count > 0)
                        {
                            DateTime timeout = _timeoutActionQueue.Peek();
                            if (DateTime.Now > timeout)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "Dequeue timeout action {0:HH:mm:ss.ffffff}, #item in queue {1}", timeout, _timeoutActionQueue.Count);
                                tuple = _timeoutActionQueue.Dequeue();
                            }
                        }
                    }
                    if (tuple != null)
                        tuple.Item2(tuple.Item1);
                    else
                        break;
                }

                bool signaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                if (signaled)
                    break;
            }
        }

        private void KeepAliveTask()
        {
            const int sleepTime = 100;
            while(true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;

                // Check player timeout + send heartbeat if needed
                foreach (IPlayer p in _playerManager.Players)
                {
                    // Check player timeout
                    if (ServerOptions.IsTimeoutDetectionActive)
                    {
                        TimeSpan timespan = DateTime.Now - p.LastActionFromClient;
                        if (timespan.TotalMilliseconds > ServerOptions.TimeoutDelay)
                        {
                            Log.WriteLine(Log.LogLevels.Info, "Timeout++ for player {0}", p.Name);
                            // Update timeout count
                            p.SetTimeout();
                            if (p.TimeoutCount >= ServerOptions.MaxTimeoutCountBeforeDisconnection)
                                OnPlayerDisconnected(p);
                        }
                    }

                    // Send heartbeat if needed
                    TimeSpan delayFromPreviousHeartbeat = DateTime.Now - p.LastActionToClient;
                    if (delayFromPreviousHeartbeat.TotalMilliseconds > ServerOptions.HeartbeatDelay)
                        p.OnPing();
                }

                bool signaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                if (signaled)
                    break;
            }
        }

        // Helpers
        private static bool IsPlayer(EntityTypes entity)
        {
            return (entity & EntityTypes.Player1) == EntityTypes.Player1
                   || (entity & EntityTypes.Player2) == EntityTypes.Player2
                   || (entity & EntityTypes.Player3) == EntityTypes.Player3
                   || (entity & EntityTypes.Player4) == EntityTypes.Player4;
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

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cancellationTokenSource != null)
                    _cancellationTokenSource.Dispose();
                if (_gameActionBlockingCollection != null)
                    _gameActionBlockingCollection.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}