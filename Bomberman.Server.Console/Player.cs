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

        public int LocationX { get; set; }
        public int LocationY { get; set; }

        public int BombRange { get; set; }
        public int BombCount { get; set; }
        public int MaxBombCount { get; set; }
        public List<EntityTypes> Bonuses { get; set; }

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

        public void OnMoved(bool succeed, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY, EntityTypes bonus)
        {
            Callback.OnMoved(succeed, oldLocationX, oldLocationY, newLocationX, newLocationY, bonus);
        }

        public void OnBombPlaced(PlaceBombResults result, EntityTypes bomb, int locationX, int locationY)
        {
            Callback.OnBombPlaced(result, bomb, locationX, locationY);
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

        public void OnEntitiesModified(List<MapModification> modifications)
        {
            Callback.OnEntitiesModified(modifications);
        }

        public void OnKilled(int playerId, EntityTypes playerEntity, int locationX, int locationY)
        {
            Callback.OnKilled(playerId, playerEntity, locationX, locationY);
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
