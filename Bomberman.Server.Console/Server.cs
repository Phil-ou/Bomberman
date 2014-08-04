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
                            p.Location.X = x;
                            p.Location.Y = y;
                        }
                    }

                    if (!positionError)
                    {
                        // TODO:

                        // Inform players about game started
                        foreach (IPlayer p in _playerManager.Players)
                            p.OnGameStarted(p.Location, GameMap);

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
            throw new NotImplementedException();
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
