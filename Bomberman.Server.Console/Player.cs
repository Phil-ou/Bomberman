using System.Collections.Generic;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console
{
    public class Player : IPlayer
    {
        public Player(string name, IBombermanCallback callback)
        {
            Name = name;
            Callback = callback;
            Location = new Location
                {
                    X = 0,
                    Y = 0
                };
        }

        #region IPlayer

        public string Name { get; private set; }

        public Location Location { get; private set; }

        public IBombermanCallback Callback { get; private set; }

        #endregion

        #region IBombermanCallback

        public void OnLogin(LoginResults result, int playerId, List<MapDescription> maps)
        {
            Callback.OnLogin(result, playerId, maps);
        }

        public void OnUserConnected(string username, int playerId)
        {
            Callback.OnUserConnected(username, playerId);
        }

        public void OnUserDisconnected(int id)
        {
            Callback.OnUserDisconnected(id);
        }

        public void OnGameStarted(Location start, Map map)
        {
            Callback.OnGameStarted(start, map);
        }

        public void OnMoved(bool succeed, Location oldLocation, Location newLocation)
        {
            Callback.OnMoved(succeed, oldLocation, newLocation);
        }

        public void OnBombPlaced()
        {
            Callback.OnBombPlaced();
        }

        public void OnChatReceived(int playerId, string msg)
        {
            Callback.OnChatReceived(playerId, msg);
        }

        public void OnEntityAdded(EntityTypes entity, Location location)
        {
            Callback.OnEntityAdded(entity, location);
        }

        public void OnEntityDeleted(EntityTypes entity, Location location)
        {
            Callback.OnEntityDeleted(entity, location);
        }

        public void OnEntityMoved(EntityTypes entity, Location oldLocation, Location newLocation)
        {
            Callback.OnEntityMoved(entity, oldLocation, newLocation);
        }

        public void OnKilled(int playerId)
        {
            Callback.OnKilled(playerId);
        }

        public void OnGameDraw()
        {
            Callback.OnGameDraw();
        }

        public void OnGameLost()
        {
            Callback.OnGameLost();
        }

        public void OnGameWon(int playerId)
        {
            Callback.OnGameWon(playerId);
        }

        public void OnPing()
        {
            Callback.OnPing();
        }

        #endregion
    }
}
