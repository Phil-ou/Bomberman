using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bomberman.Client.Interfaces;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Console
{
    public class Client : IBombermanCallback
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;

        private IProxy _proxy;
        private readonly ConsoleUI _consoleUI;
        private DateTime _lastActionFromServer;
        private int _timeoutCount;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _keepAliveTask;

        public List<MapDescription> MapDescriptions { get; private set; }
        public List<Opponent> Opponents { get; private set; }

        public Map GameMap { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsPlaying { get; private set; }
        public string Name { get; private set; }
        public int Id { get; private set; }
        public EntityTypes Entity { get; private set; }
        public int LocationX { get; private set; }
        public int LocationY { get; private set; }

        public Client(ConsoleUI consoleUI)
        {
            _consoleUI = consoleUI;

            IsConnected = false;
            IsPlaying = false;

            _cancellationTokenSource = new CancellationTokenSource();
            _keepAliveTask = Task.Factory.StartNew(KeepAliveTask, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _keepAliveTask.Wait(1000);
        }

        public void Connect(IProxy proxy, string name)
        {
            if (proxy == null)
                throw new ArgumentNullException("proxy");
            if (name == null)
                throw new ArgumentNullException("name");

            Log.WriteLine(Log.LogLevels.Debug, "Connecting as {0}", name);

            if (IsConnected)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot connect, already connected");
                return;
            }

            Opponents = new List<Opponent>();
            Name = name;

            _proxy = proxy;
            _proxy.OnConnectionLost += OnConnectionLost;
            _proxy.Login(name);
        }

        public void Chat(string msg)
        {
            if (msg == null)
                throw new ArgumentNullException("msg");

            if (!IsConnected)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot send chat, not connected to server");
                return;
            }

            Log.WriteLine(Log.LogLevels.Debug, "Sending chat: {0}", msg);

            _proxy.Chat(msg);
        }

        public void StartGame(int mapId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Starting game");

            if (IsConnected)
            {
                if (!IsPlaying)
                    _proxy.StartGame(mapId);
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Game already started");
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Cannot start game, not connected to server");
        }

        public void Move(Directions direction)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Moving to {0}", direction);

            if (IsConnected)
            {
                if (IsPlaying)
                    _proxy.Move(direction);
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Game not started");
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Cannot move, not connected to server");
        }

        public void PlaceBomb()
        {
            Log.WriteLine(Log.LogLevels.Debug, "Placing bomb");

            if (IsConnected)
            {
                if (IsPlaying)
                    _proxy.PlaceBomb();
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Game not started");
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Cannot place bomb, not connected to server");
        }

        private void OnConnectionLost()
        {
            Id = -1;
            IsPlaying = false;

            Disconnect();

            _consoleUI.OnConnectionLost();
        }

        private void Disconnect()
        {
            Id = -1;
            IsPlaying = false;

            if (_proxy != null)
            {
                _proxy.OnConnectionLost -= OnConnectionLost;
                _proxy.Disconnect();
                _proxy = null;
            }
        }

        private void ResetTimeout()
        {
            _timeoutCount = 0;
            _lastActionFromServer = DateTime.Now;
        }

        private void SetTimeout()
        {
            _timeoutCount++;
            _lastActionFromServer = DateTime.Now;
        }

        #region IBombermanCallback

        public void OnLogin(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnLogin: Id {0} Result: {1}", playerId, result);
            if (result == LoginResults.Successful)
            {
                IsConnected = true;
                Id = playerId;
                Entity = playerEntity;
                MapDescriptions = maps;

                string mapsAsString = maps == null ? String.Empty : maps.Select(x => String.Format("[{0},{1},{2}]", x.Id, x.Title, x.Size)).Aggregate((s, s1) => s + s1);
                _consoleUI.OnLogin(playerId, playerEntity, mapsAsString);
            }
            else
            {
                IsConnected = false;
                Log.WriteLine(Log.LogLevels.Warning, "Cannot connect to server with name {0}: {1}", Name, result);
            }
        }

        public void OnUserConnected(string username, int playerId)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnUserConnected: Player {0}|{1}", username, playerId);

            Opponents.Add(new Opponent
                {
                    Id = playerId,
                    Name = username,
                });
            _consoleUI.OnUserConnected(username, playerId);
        }

        public void OnUserDisconnected(int playerId)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnUserDisconnected: Player {0}", playerId);

            Opponent opponent = Opponents.FirstOrDefault(x => x.Id == playerId);
            if (opponent != null)
            {
                Opponents.Remove(opponent);
                _consoleUI.OnUserDisconnected(opponent.Name, opponent.Id);
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Unknown disconnected player {0}", playerId);
        }

        public void OnGameStarted(int locationX, int locationY, Map map)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnGameStarted: start:{0},{1} map:{2}, {3}", locationX, locationY, map.Description.Id, map.Description.Title);

            // TODO
            LocationX = locationX;
            LocationY = locationY;
            GameMap = map;
            IsPlaying = true;

            _consoleUI.OnGameStarted(map);
        }

        public void OnMoved(bool succeed, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnMoved: {0}: {1},{2} -> {3},{4}", succeed, oldLocationX, oldLocationY, newLocationX, newLocationY);

            if (succeed)
            {
                // Move ourself in map
                GameMap.DeleteEntity(oldLocationX, oldLocationY, Entity);
                GameMap.AddEntity(newLocationX, newLocationY, Entity);

                //
                _consoleUI.OnEntityMoved(Entity, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Set new location
                LocationX = newLocationX;
                LocationY = newLocationY;
            }
        }

        public void OnBombPlaced(PlaceBombResults result, EntityTypes bomb, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnBombPlaced: succeed {0} -> {1},{2}: {3}", result, locationX, locationY, bomb);

            if (result == PlaceBombResults.Successful)
            {
                // Add bomb to map
                GameMap.AddEntity(locationX, locationY, bomb);

                //
                _consoleUI.OnEntityAdded(bomb, locationX, locationY);
            }
        }

        public void OnBonusPickedUp(EntityTypes bonus, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnBonusPickedUp: {0}", bonus);

            // Delete bonus in map
            GameMap.DeleteEntity(locationX, locationY, bonus);
            // TODO: add bonus to bonus list + display bonus list

            // TODO: add bonus to bonus list + display bonus list
        }

        public void OnChatReceived(int playerId, string msg)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnChatReceived: {0} {1}", playerId, msg);

            if (playerId == Id)
                _consoleUI.OnChat(Name, msg);
            else
            {
                Opponent player = Opponents.FirstOrDefault(x => x.Id == playerId);
                if (player != null)
                    _consoleUI.OnChat(player.Name, msg);
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Msg {0} received from unknown player {1}", msg, playerId);
            }
        }

        public void OnEntityAdded(EntityTypes entity, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityAdded: entity {0}: {1},{2}", entity, locationX, locationY);

            // Delete entity from map
            GameMap.AddEntity(locationX, locationY, entity);

            //
            _consoleUI.OnEntityAdded(entity, locationX, locationY);
        }

        public void OnEntityDeleted(EntityTypes entity, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityDeleted: entity {0}: {1},{2}", entity, locationX, locationY);

            // Delete entity from map
            GameMap.DeleteEntity(locationX, locationY, entity);

            //
            _consoleUI.OnEntityDeleted(entity, locationX, locationY);
        }

        public void OnEntityMoved(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityMoved: entity {0}: {1},{2} -> {3},{4}", entity, oldLocationX, oldLocationY, newLocationX, newLocationY);

            // Move entity in map
            GameMap.DeleteEntity(oldLocationX, oldLocationY, entity);
            GameMap.AddEntity(newLocationX, newLocationY, entity);

            //
            _consoleUI.OnEntityMoved(entity, oldLocationX, oldLocationY, newLocationX, newLocationY);
        }

        public void OnEntityTransformed(EntityTypes oldEntity, EntityTypes newEntity, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityTransformed: {0},{1}: {2} -> {3}", locationX, locationY, oldEntity, newEntity);

            // Transform entity in map
            GameMap.DeleteEntity(locationX, locationY, oldEntity);
            GameMap.AddEntity(locationX, locationY, newEntity);

            //
            _consoleUI.OnEntityTransformed(oldEntity, newEntity, locationX, locationY);
        }

        public void OnEntitiesModified(List<MapModification> modifications)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntitiesModified: count:{0}", modifications.Count);

            foreach(MapModification modification in modifications)
            switch(modification.Action)
            {
                case MapModificationActions.Add:
                    GameMap.AddEntity(modification.X, modification.Y, modification.Entity);
                    break;
                    case MapModificationActions.Delete:
                    GameMap.DeleteEntity(modification.X, modification.Y, modification.Entity);
                    break;
                default:
                    Log.WriteLine(Log.LogLevels.Error, "Invalid modification action: {0}", modification.Action);
                    break;
            }

            //
            _consoleUI.Redraw();
        }

        public void OnKilled(int playerId, EntityTypes playerEntity, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnKilled: {0} {1} : {2},{3}", playerId, playerEntity, locationX, locationY);

            Opponent player = Opponents.FirstOrDefault(x => x.Id == playerId);
            if (player != null)
            {
                GameMap.DeleteEntity(locationX, locationY, playerEntity);
                OnEntityDeleted(playerEntity, locationX, locationY);
                _consoleUI.OnKilled(player.Name);
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Unknown player killed {0}", playerId);
        }

        public void OnGameDraw()
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnGameDraw");

            IsPlaying = false;
            _consoleUI.OnGameDraw();
        }

        public void OnGameLost()
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnGameLost");

            IsPlaying = false;
            _consoleUI.OnGameLost();
        }

        public void OnGameWon(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnGameWon {0}", playerId);

            ResetTimeout();
            IsPlaying = false;
            if (playerId == Id)
                _consoleUI.OnGameWon(true, Name);
            else
            {
                Opponent player = Opponents.FirstOrDefault(x => x.Id == playerId);
                if (player != null)
                    _consoleUI.OnGameWon(false, player.Name);
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Game won by an unknown player {0}", playerId);
            }
        }

        public void OnPing()
        {
            ResetTimeout();
        }

        #endregion

        private void KeepAliveTask()
        {
            const int sleepTime = 100;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;

                // Check server timeout
                TimeSpan timespan = DateTime.Now - _lastActionFromServer;
                if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
                {
                    Log.WriteLine(Log.LogLevels.Debug, "Timeout++");
                    // Update timeout count
                    SetTimeout();
                    if (_timeoutCount >= MaxTimeoutCountBeforeDisconnection)
                        OnConnectionLost(); // timeout
                }

                // Send heartbeat if needed
                if (_proxy != null)
                {
                    TimeSpan delaySinceLastActionToServer = DateTime.Now - _proxy.LastActionToServer;
                    if (delaySinceLastActionToServer.TotalMilliseconds > HeartbeatDelay)
                        _proxy.Heartbeat();
                }

                bool signaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                if (signaled)
                    break;
            }
        }
    }
}
