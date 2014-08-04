using System;
using System.Collections.Generic;
using System.Linq;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Console
{
    public class Client : IBombermanCallback
    {
        private WCFProxy _proxy;

        public List<MapDescription> MapDescriptions { get; private set; }
        public List<Opponent> Opponents { get; private set; }

        public bool IsConnected { get; private set; }
        public string Name { get; private set; }
        public int Id { get; private set; }

        public Client()
        {
            IsConnected = false;
        }

        public void Connect(WCFProxy proxy, string name)
        {
            if (proxy == null)
                throw new ArgumentNullException("proxy");
            if (name == null)
                throw new ArgumentNullException("name");

            Log.WriteLine(Log.LogLevels.Info, "Connecting as {0}", name);

            if (IsConnected)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot connect, already connected");
                return;
            }

            Opponents = new List<Opponent>();
            Name = name;

            _proxy = proxy;
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

            Log.WriteLine(Log.LogLevels.Info, "Sending chat: {0}", msg);

            _proxy.Chat(msg);
        }

        public void StartGame(int mapId)
        {
            Log.WriteLine(Log.LogLevels.Info, "Starting game");
            
            if (!IsConnected)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot start game, not connected to server");
                return;
            }

            _proxy.StartGame(mapId);
        }

        #region IBombermanCallback

        public void OnLogin(LoginResults result, int playerId, List<MapDescription> maps)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnLogin: Id {0} Result: {1}", playerId, result);
            if (result == LoginResults.Successful)
            {
                Id = playerId;
                IsConnected = true;

                MapDescriptions = maps;

                string mapsAsString = maps == null ? String.Empty : maps.Select(x => String.Format("[{0},{1},{2}]", x.Id, x.Title, x.Size)).Aggregate((s, s1) => s + s1);

                Log.WriteLine(Log.LogLevels.Info, "Login successful as {0}. Maps: {1}", playerId, mapsAsString);
            }
            else
            {
                IsConnected = false;
                Log.WriteLine(Log.LogLevels.Warning, "Cannot connect to server with name {0}: {1}", Name, result);
            }
        }

        public void OnUserConnected(string username, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnUserConnected: Player {0}|{1}", username, playerId);
            Opponents.Add(new Opponent
                {
                    Id = playerId,
                    Name = username,
                });
            Log.WriteLine(Log.LogLevels.Info, "New player {0}|{1} connected", username, playerId);
        }

        public void OnUserDisconnected(int id)
        {
            throw new NotImplementedException();
        }

        public void OnGameStarted(Location start, Map map)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnGameStarted: start:{0},{1} map:{2}, {3}", start.X, start.Y, map.Description.Id, map.Description.Title);
            
            // TODO

            Log.WriteLine(Log.LogLevels.Info, "Game started, location: {0},{1} Map: {2},{3}", start.X, start.Y, map.Description.Id, map.Description.Title);
        }

        public void OnMoved(bool succeed, Location oldLocation, Location newLocation)
        {
            throw new NotImplementedException();
        }

        public void OnBombPlaced()
        {
            throw new NotImplementedException();
        }

        public void OnChatReceived(int playerId, string msg)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnChatReceived: {0} {1}", playerId, msg);
            // TODO
            Log.WriteLine(Log.LogLevels.Info, "Chat received from {0}: {1}", playerId, msg);
        }

        public void OnEntityAdded(EntityTypes entity, Location location)
        {
            throw new NotImplementedException();
        }

        public void OnEntityDeleted(EntityTypes entity, Location location)
        {
            throw new NotImplementedException();
        }

        public void OnEntityMoved(EntityTypes entity, Location oldLocation, Location newLocation)
        {
            throw new NotImplementedException();
        }

        public void OnKilled(int playerId)
        {
            throw new NotImplementedException();
        }

        public void OnGameDraw()
        {
            throw new NotImplementedException();
        }

        public void OnGameLost()
        {
            throw new NotImplementedException();
        }

        public void OnGameWon(int playerId)
        {
            throw new NotImplementedException();
        }

        public void OnPing()
        {
            // TODO: refresh timeout
            throw new NotImplementedException();
        }

        #endregion
    }
}
