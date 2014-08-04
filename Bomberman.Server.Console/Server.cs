using System;
using System.Linq;
using Bomberman.Common;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console
{
    public enum ServerStates
    {
        WaitStartGame,
        GameStarted,
    }

    public class Server
    {
        private readonly WCFHost _host;
        private readonly IPlayerManager _playerManager;
        private readonly IMapManager _mapManager;

        public ServerStates State { get; private set; }
        public Map GameMap { get; private set; }

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
            _host.OnMove += OnMove;
            _host.OnPlaceBomb += OnPlaceBomb;
            _host.OnChat += OnChat;

            State = ServerStates.WaitStartGame;
        }

        public void Start()
        {
            _host.Start();
        }

        public void Stop()
        {
            _host.Stop();
        }

        private void OnLogin(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "New player {0}|{1} connected", player.Name, playerId);

            // Inform player about its playerId
            player.OnLogin(LoginResults.Successful, playerId, _mapManager.MapDescriptions);

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
                GameMap = _mapManager.Maps.FirstOrDefault(x => x.Description.Id == mapId);
                if (GameMap != null)
                {
                    bool positionError = false;
                    // Set players position
                    foreach(IPlayer p in _playerManager.Players)
                    {
                        EntityTypes entityType = EntityTypes.Empty;
                        int id = _playerManager.GetId(p);
                        if (id == 0)
                            entityType = EntityTypes.Player1;
                        else if (id==1)
                            entityType = EntityTypes.Player2;
                        else if (id==2)
                            entityType = EntityTypes.Player3;
                        else if (id==3)
                            entityType = EntityTypes.Player4;
                        var entry = GameMap.MapAsArray.Select((entity, index) => new
                            {
                                entity,
                                index
                            }).FirstOrDefault(x => x.entity == entityType);
                        if (entry == null)
                        {
                            Log.WriteLine(Log.LogLevels.Error, "Cannot find position of {0} in map {1}", entityType, GameMap.Description.Id);
                            positionError = true;
                            break;
                        }
                        else
                        {
                            // Set player position
                            int x = entry.index % GameMap.Description.Size;
                            int y = entry.index / GameMap.Description.Size;
                            p.LocationX = x;
                            p.LocationY = y;
                        }
                    }

                    if (!positionError)
                    {
                        // TODO:

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

        private void OnMove(IPlayer player, Directions direction)
        {
            Log.WriteLine(Log.LogLevels.Info, "Move {0} to {1}", player.Name, direction);

            // TODO: queue action

            // Get old coordinates
            int oldLocationX = player.LocationX;
            int oldLocationY = player.LocationY;
            int oldLocationIndex = oldLocationY*GameMap.Description.Size + oldLocationX;

            // Get new coordinates
            int stepX = 0, stepY = 0;
            switch(direction)
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
            }
            int newLocationX = oldLocationX + stepX;
            int newLocationY = oldLocationY + stepY;
            int newLocationIndex = newLocationY*GameMap.Description.Size + newLocationX;

            // Check if collider on new location
            EntityTypes collider = GameMap.MapAsArray[newLocationIndex];
            // TODO: handle other entityType
            if (collider == EntityTypes.Empty) // can only move to empty location
            {
                Log.WriteLine(Log.LogLevels.Debug, "Moved successfully from {0},{1} to {2},{3}", oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Set new location
                player.LocationX = newLocationX;
                player.LocationY = newLocationY;

                // Get player entity
                EntityTypes entity = EntityTypes.Empty;
                int id = _playerManager.GetId(player);
                switch (id)
                {
                    case 0:
                        entity = EntityTypes.Player1;
                        break;
                    case 1:
                        entity = EntityTypes.Player2;
                        break;
                    case 2:
                        entity = EntityTypes.Player3;
                        break;
                    case 3:
                        entity = EntityTypes.Player4;
                        break;
                }

                // Move player on map
                GameMap.MapAsArray[oldLocationIndex] ^= entity;
                GameMap.MapAsArray[newLocationIndex] ^= entity;

                // Inform player about its new location
                player.OnMoved(true, oldLocationX, oldLocationY, newLocationX, newLocationY);

                // Inform other player about player new location
                foreach(IPlayer other in _playerManager.Players.Where(x => x != player))
                    other.OnEntityMoved(entity, oldLocationX, oldLocationY, newLocationX, newLocationY);
            }
            else
                player.OnMoved(false, -1, -1, -1, -1);
        }

        private void OnPlaceBomb(IPlayer player)
        {
            throw new NotImplementedException();
        }

        private void OnChat(IPlayer player, string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "Chat from {0}:{1}", player.Name, msg);

            // Send message to other players
            int id = _playerManager.GetId(player);
            foreach(IPlayer other in _playerManager.Players.Where(x => x != player))
                other.OnChatReceived(id, msg);
        }
    }
}
