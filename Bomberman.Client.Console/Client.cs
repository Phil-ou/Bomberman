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
        private readonly ConsoleUI _consoleUI;

        public List<MapDescription> MapDescriptions { get; private set; }
        public List<Opponent> Opponents { get; private set; }

        public Map Map { get; private set; }
        public bool IsConnected { get; private set; }
        public string Name { get; private set; }
        public int Id { get; private set; }
        public EntityTypes Entity { get; private set; }
        public int LocationX { get; private set; }
        public int LocationY { get; private set; }

        public Client(ConsoleUI consoleUI)
        {
            _consoleUI = consoleUI;

            IsConnected = false;
        }

        public void Connect(WCFProxy proxy, string name)
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
            
            if (!IsConnected)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Cannot start game, not connected to server");
                return;
            }

            _proxy.StartGame(mapId);
        }

        public void Move(Directions direction)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Moving to {0}", direction);

            _proxy.Move(direction);
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

                switch(playerId)
                {
                    case 0: 
                        Entity = EntityTypes.Player1;
                        break;
                    case 1:
                        Entity = EntityTypes.Player2;
                        break;
                    case 2:
                        Entity = EntityTypes.Player3;
                        break;
                    case 3:
                        Entity = EntityTypes.Player4;
                        break;
                }

                _consoleUI.OnLogin(playerId, mapsAsString);
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
            _consoleUI.OnUserConnected(username, playerId);
        }

        public void OnUserDisconnected(int id)
        {
            throw new NotImplementedException();
        }

        public void OnGameStarted(int locationX, int locationY, Map map)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnGameStarted: start:{0},{1} map:{2}, {3}", locationX, locationY, map.Description.Id, map.Description.Title);
            
            // TODO
            LocationX = locationX;
            LocationY = locationY;
            Map = map;

            _consoleUI.OnGameStarted(map);
        }

        public void OnMoved(bool succeed, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnMoved: succeed {0} {1},{2} -> {3},{4}", succeed, oldLocationX, oldLocationY, newLocationX, newLocationY);

            if (succeed)
            {
                int oldLocationIndex = oldLocationY*Map.Description.Size + oldLocationX;
                int newLocationIndex = newLocationY * Map.Description.Size + newLocationX;

                Map.MapAsArray[oldLocationIndex] ^= Entity;
                Map.MapAsArray[newLocationIndex] |= Entity;

                _consoleUI.OnEntityMoved(Entity, oldLocationX, oldLocationY, newLocationX, newLocationY);

                LocationX = newLocationX;
                LocationY = newLocationY;
            }
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

        public void OnEntityAdded(EntityTypes entity, int locationX, int locationY)
        {
            throw new NotImplementedException();
        }

        public void OnEntityDeleted(EntityTypes entity, int locationX, int locationY)
        {
            throw new NotImplementedException();
        }

        public void OnEntityMoved(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            Log.WriteLine(Log.LogLevels.Debug, "OnEntityMoved: entity {0} {1},{2} -> {3},{4}", entity, oldLocationX, oldLocationY, newLocationX, newLocationY);

            int oldLocationIndex = oldLocationY * Map.Description.Size + oldLocationX;
            int newLocationIndex = newLocationY * Map.Description.Size + newLocationX;

            Map.MapAsArray[oldLocationIndex] ^= entity;
            Map.MapAsArray[newLocationIndex] |= entity;

            _consoleUI.OnEntityMoved(Entity, oldLocationX, oldLocationY, newLocationX, newLocationY);
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
