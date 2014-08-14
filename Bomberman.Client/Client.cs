using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bomberman.Client.Interfaces;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;
using Bomberman.Common.Helpers;

namespace Bomberman.Client
{
    internal enum States
    {
        Created, // -> LoggingIn
        LoggingIn, // -> Logged
        Logged, // --> Playing
        Playing, // -> Logged
    }

    public class Client : IBombermanCallback, IClient
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;

        private IProxy _proxy;
        private DateTime _lastActionFromServer;
        private int _timeoutCount;
        private States _state;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _keepAliveTask;

        public Client()
        {
            _state = States.Created;

            _cancellationTokenSource = new CancellationTokenSource();
            _keepAliveTask = Task.Factory.StartNew(KeepAliveTask, _cancellationTokenSource.Token);
        }

        #region IClient

        public bool IsConnected
        {
            get { return _state == States.Logged || _state == States.Playing; }
        }

        public bool IsPlaying
        {
            get { return _state == States.Playing; }
        }

        public List<MapDescription> MapDescriptions { get; private set; }
        public List<IOpponent> Opponents { get; private set; }

        public Map GameMap { get; private set; }
        public string Name { get; private set; }
        public int Id { get; private set; }
        public EntityTypes Entity { get; private set; }
        public int LocationX { get; private set; }
        public int LocationY { get; private set; }

        public event LoginEventHandler LoggedOn;
        public event UserConnectedEventHandler UserConnected;
        public event UserDisconnectedEventHandler UserDisconnected;
        public event GameStartedEventHandler GameStarted;
        public event BonusPickedUpEventHandler BonusPickedUp;
        public event ChatReceivedEventHandler ChatReceived;
        public event EntityAddedEventHandler EntityAdded;
        public event EntityDeletedEventHandler EntityDeleted;
        public event EntityMovedEventHandler EntityMoved;
        public event EntityTransformedEventHandler EntityTransformed;
        public event MultipleEntityModifiedEventHandler MultipleEntityModified;
        public event GameDrawEventHandler GameDraw;
        public event GameLostEventHandler GameLost;
        public event GameWonEventHandler GameWon;
        public event KilledEventHandler Killed;
        public event ConnectionLostEventHandler ConnectionLost;

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _keepAliveTask.Wait(1000);
        }

        public void Login(Func<IBombermanCallback, IProxy> createProxyFunc, string name)
        {
            if (createProxyFunc == null)
                throw new ArgumentNullException("createProxyFunc");
            if (name == null)
                throw new ArgumentNullException("name");

            Log.WriteLine(Log.LogLevels.Debug, "Connecting as {0}", name);

            if (_state != States.Created)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot connect, already connected");
                return;
            }

            _state = States.LoggingIn;

            Opponents = new List<IOpponent>();
            Name = name;

            _proxy = createProxyFunc(this);
            _proxy.ConnectionLost += OnConnectionLost;
            _proxy.Login(name);
        }

        public void Logout()
        {
            Log.WriteLine(Log.LogLevels.Debug, "Disconnect");

            if (_state == States.Created || _state == States.LoggingIn)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot disconnect, not yet connected");
                return;
            }

            _proxy.Logout();
            InternalDisconnect();
        }

        public void Chat(string msg)
        {
            if (msg == null)
                throw new ArgumentNullException("msg");

            Log.WriteLine(Log.LogLevels.Debug, "Sending chat: {0}", msg);

            if (_state != States.Logged || _state != States.Playing)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot send chat, not connected to server");
                return;
            }

            _proxy.Chat(msg);
        }

        public void StartGame(int mapId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Starting game");

            if (_state != States.Logged)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot start game, not connected or game not started");
                return;
            }
            _proxy.StartGame(mapId);
        }

        public void Move(Directions direction)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Moving to {0}", direction);

            if (_state != States.Playing)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot move, not connected or game not started");
                return;
            }
            _proxy.Move(direction);
        }

        public void PlaceBomb()
        {
            Log.WriteLine(Log.LogLevels.Debug, "Placing bomb");

            if (_state != States.Playing)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot place bomb, not connected or game not started");
                return;
            }
            _proxy.PlaceBomb();
        }

        #endregion

        #region IBombermanCallback

        public void OnLogin(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps, bool isGameStarted)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnLogin: Id {0} Result: {1} Entity: {2} GameStarted: {3}", playerId, result, playerEntity, isGameStarted);
            if (result == LoginResults.Successful)
            {
                _state = States.Logged;
                Id = playerId;
                Entity = playerEntity;
                MapDescriptions = maps;

            }
            else
            {
                _state = States.Created;
                Log.WriteLine(Log.LogLevels.Warning, "Cannot connect to server with name {0}: {1}", Name, result);
            }

            LoggedOn.Do(x => x(result, playerId, playerEntity, maps, isGameStarted));
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

            UserConnected.Do(x => x(username, playerId));
        }

        public void OnUserDisconnected(int playerId)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnUserDisconnected: Player {0}", playerId);

            IOpponent opponent = Opponents.FirstOrDefault(x => x.Id == playerId);
            if (opponent != null)
            {
                Opponents.Remove(opponent);

                UserDisconnected.Do(x => x(opponent.Name, opponent.Id));
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Unknown disconnected player {0}", playerId);
        }

        public void OnGameStarted(int locationX, int locationY, Map map)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnGameStarted: start:{0},{1} map:{2}, {3} started:{4}", locationX, locationY, map.Description.Id, map.Description.Title);

            LocationX = locationX;
            LocationY = locationY;
            GameMap = map;
            _state = States.Playing;

            GameStarted.Do(x => x(map));
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
                EntityMoved.Do(x => x(Entity, oldLocationX, oldLocationY, newLocationX, newLocationY));

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
                EntityAdded.Do(x => x(bomb, locationX, locationY));
            }
        }

        public void OnBonusPickedUp(EntityTypes bonus, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnBonusPickedUp: {0}", bonus);

            // Delete bonus in map
            GameMap.DeleteEntity(locationX, locationY, bonus);

            //
            EntityDeleted.Do(x => x(bonus, locationX, locationY));

            // TODO: add bonus to bonus list + display bonus list
            BonusPickedUp.Do(x => x(bonus));
        }

        public void OnChatReceived(int playerId, string msg)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnChatReceived: {0} {1}", playerId, msg);

            if (playerId == Id)
                ChatReceived.Do(x => x(playerId, Name, msg));
            else
            {
                IOpponent opponent = Opponents.FirstOrDefault(x => x.Id == playerId);
                if (opponent != null)
                    ChatReceived.Do(x => x(playerId, opponent.Name, msg));
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
            EntityAdded.Do(x => x(entity, locationX, locationY));
        }

        public void OnEntityDeleted(EntityTypes entity, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityDeleted: entity {0}: {1},{2}", entity, locationX, locationY);

            // Delete entity from map
            GameMap.DeleteEntity(locationX, locationY, entity);

            //
            EntityDeleted.Do(x => x(entity, locationX, locationY));
        }

        public void OnEntityMoved(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityMoved: entity {0}: {1},{2} -> {3},{4}", entity, oldLocationX, oldLocationY, newLocationX, newLocationY);

            // Move entity in map
            GameMap.DeleteEntity(oldLocationX, oldLocationY, entity);
            GameMap.AddEntity(newLocationX, newLocationY, entity);

            //
            EntityMoved.Do(x => x(entity, oldLocationX, oldLocationY, newLocationX, newLocationY));
        }

        public void OnEntityTransformed(EntityTypes oldEntity, EntityTypes newEntity, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityTransformed: {0},{1}: {2} -> {3}", locationX, locationY, oldEntity, newEntity);

            // Transform entity in map
            GameMap.DeleteEntity(locationX, locationY, oldEntity);
            GameMap.AddEntity(locationX, locationY, newEntity);

            //
            EntityTransformed.Do(x => x(oldEntity, newEntity, locationX, locationY));
        }

        public void OnEntitiesModified(List<MapModification> modifications)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnEntitiesModified: count:{0}", modifications.Count);

            foreach (MapModification modification in modifications)
                switch (modification.Action)
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
            MultipleEntityModified.Do(x => x());
        }

        public void OnKilled(int playerId, EntityTypes playerEntity, int locationX, int locationY)
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnKilled: {0} {1} : {2},{3}", playerId, playerEntity, locationX, locationY);

            IOpponent player = Opponents.FirstOrDefault(x => x.Id == playerId);
            if (player != null)
            {
                GameMap.DeleteEntity(locationX, locationY, playerEntity);
                OnEntityDeleted(playerEntity, locationX, locationY);

                Killed.Do(x => x(player.Name));
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "Unknown player killed {0}", playerId);
        }

        public void OnGameDraw()
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnGameDraw");

            _state = States.Logged;
            GameDraw.Do(x => x());
        }

        public void OnGameLost()
        {
            ResetTimeout();
            Log.WriteLine(Log.LogLevels.Debug, "OnGameLost");

            _state = States.Logged;
            GameLost.Do(x => x());
        }

        public void OnGameWon(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnGameWon {0}", playerId);

            ResetTimeout();
            _state = States.Logged;
            if (playerId == Id)
                GameWon.Do(x => x(true, Name));
            else
            {
                IOpponent player = Opponents.FirstOrDefault(x => x.Id == playerId);
                if (player != null)
                    GameWon.Do(x => x(false, player.Name));
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Game won by an unknown player {0}", playerId);
            }
        }

        public void OnPing()
        {
            ResetTimeout();
        }

        #endregion

        private void OnConnectionLost()
        {
            Id = -1;
            _state = States.Created;

            InternalDisconnect();

            ConnectionLost.Do(x => x());
        }

        private void InternalDisconnect()
        {
            Id = -1;
            _state = States.Created;

            if (_proxy != null)
            {
                _proxy.ConnectionLost -= OnConnectionLost;
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

        // Task
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
