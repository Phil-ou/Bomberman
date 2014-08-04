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
            LocationX = -1;
            LocationY = -1;
        }

        #region IPlayer

        public string Name { get; private set; }

        public int LocationX { get; set; }
        public int LocationY { get; set; }

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

        public void OnGameStarted(int locationX, int locationY, Map map)
        {
            Callback.OnGameStarted(locationX, locationY, map);
        }

        public void OnMoved(bool succeed, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            Callback.OnMoved(succeed, oldLocationX, oldLocationY, newLocationX, newLocationY);
        }

        public void OnBombPlaced()
        {
            Callback.OnBombPlaced();
        }

        public void OnChatReceived(int playerId, string msg)
        {
            Callback.OnChatReceived(playerId, msg);
        }

        public void OnEntityAdded(EntityTypes entity, int locationX, int locationY)
        {
            Callback.OnEntityAdded(entity, locationX, locationY);
        }

        public void OnEntityDeleted(EntityTypes entity, int locationX, int locationY)
        {
            Callback.OnEntityDeleted(entity, locationX, locationY);
        }

        public void OnEntityMoved(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            Callback.OnEntityMoved(entity, oldLocationX, oldLocationY, newLocationX, newLocationY);
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
