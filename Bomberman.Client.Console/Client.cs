using System;
using System.Collections.Generic;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Console
{
    public class Client : IBombermanCallback
    {
        private WCFProxy _proxy;

        public Client()
        {
        }

        public void Connect(WCFProxy proxy, string name)
        {
            if (proxy == null)
                throw new ArgumentNullException("proxy");
            if (name == null)
                throw new ArgumentNullException("name");

            _proxy = proxy;
            _proxy.Login(name);
        }

        #region IBombermanCallback

        public void OnLogin(LoginResults result, int playerId, List<MapDescription> maps)
        {
            throw new NotImplementedException();
        }

        public void OnUserConnected(string username, int playerId)
        {
            throw new NotImplementedException();
        }

        public void OnUserDisconnected(int id)
        {
            throw new NotImplementedException();
        }

        public void OnGameStarted(Location start, Map map)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
