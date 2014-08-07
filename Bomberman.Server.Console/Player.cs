using System.Collections.Generic;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;
using Bomberman.Server.Console.Interfaces;

namespace Bomberman.Server.Console
{
    // TODO: wrap every call to callback by an exception handler
    public class Player : IPlayer
    {
        public Player(string name, IBombermanCallback callback)
        {
            Name = name;
            Callback = callback;
            LocationX = -1;
            LocationY = -1;
            State = PlayerStates.Connected;
        }

        #region IPlayer

        public string Name { get; private set; }

        public PlayerStates State { get; set; }

        public Directions Direction { get; set; }
        public int LocationX { get; set; }
        public int LocationY { get; set; }

        public EntityTypes PlayerEntity { get; set; }

        public IBombermanCallback Callback { get; private set; }

        #endregion

        #region IBombermanCallback

        public void OnLogin(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps)
        {
            Callback.OnLogin(result, playerId, playerEntity, maps);
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

        public void OnBombPlaced(bool succeed, EntityTypes bomb, int locationX, int locationY)
        {
            Callback.OnBombPlaced(succeed, bomb, locationX, locationY);
        }

        public void OnBonusPickedUp(EntityTypes bonus)
        {
            Callback.OnBonusPickedUp(bonus);
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

        public void OnEntityTransformed(EntityTypes oldEntity, EntityTypes newEntity, int locationX, int locationY)
        {
            Callback.OnEntityTransformed(oldEntity, newEntity, locationX, locationY);
        }

        public void OnMapModified(List<MapModification> modifications)
        {
            Callback.OnMapModified(modifications);
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
