using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;
using Bomberman.Common.Helpers;
using Bomberman.Server.Interfaces;

namespace Bomberman.Server.PlayerManager
{
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

        private void ExceptionFreeAction(Action action, [CallerMemberName]string actionName = null)
        {
            try
            {
                action();
                LastActionToClient = DateTime.Now;
            }
            catch (CommunicationObjectAbortedException)
            {
                ConnectionLost.Do(x => x(this));
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLost.Do(x => x(this));
            }
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

        public event ConnectionLostEventHandler ConnectionLost;

        // Heartbeat management
        public DateTime LastActionToClient { get; private set; } // used to check if heartbeat is needed
        // Timeout management
        public DateTime LastActionFromClient { get; private set; }
        public int TimeoutCount { get; private set; }

        public void ResetTimeout()
        {
            TimeoutCount = 0;
            LastActionFromClient = DateTime.Now;
        }

        public void SetTimeout()
        {
            TimeoutCount++;
            LastActionFromClient = DateTime.Now;
        }

        #endregion

        #region IBombermanCallback

        public void OnLogin(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps, bool isGameStarted)
        {
            ExceptionFreeAction(() => Callback.OnLogin(result, playerId, playerEntity, maps, isGameStarted));
        }

        public void OnUserConnected(string username, int playerId)
        {
            ExceptionFreeAction(() => Callback.OnUserConnected(username, playerId));
        }

        public void OnUserDisconnected(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnUserDisconnected(playerId));
        }

        public void OnGameStarted(int locationX, int locationY, Map map)
        {
            ExceptionFreeAction(() => Callback.OnGameStarted(locationX, locationY, map));
        }

        public void OnMoved(bool succeed, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            ExceptionFreeAction(() => Callback.OnMoved(succeed, oldLocationX, oldLocationY, newLocationX, newLocationY));
        }

        public void OnBombPlaced(PlaceBombResults result, EntityTypes bomb, int locationX, int locationY)
        {
            ExceptionFreeAction(() => Callback.OnBombPlaced(result, bomb, locationX, locationY));
        }

        public void OnBonusPickedUp(EntityTypes bonus, int locationX, int locationY)
        {
            ExceptionFreeAction(() => Callback.OnBonusPickedUp(bonus, locationX, locationY));
        }

        public void OnChatReceived(int playerId, string msg)
        {
            ExceptionFreeAction(() => Callback.OnChatReceived(playerId, msg));
        }

        public void OnEntityAdded(EntityTypes entity, int locationX, int locationY)
        {
            ExceptionFreeAction(() => Callback.OnEntityAdded(entity, locationX, locationY));
        }

        public void OnEntityDeleted(EntityTypes entity, int locationX, int locationY)
        {
            ExceptionFreeAction(() => Callback.OnEntityDeleted(entity, locationX, locationY));
        }

        public void OnEntityMoved(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            ExceptionFreeAction(() => Callback.OnEntityMoved(entity, oldLocationX, oldLocationY, newLocationX, newLocationY));
        }

        public void OnEntityTransformed(EntityTypes oldEntity, EntityTypes newEntity, int locationX, int locationY)
        {
            ExceptionFreeAction(() => Callback.OnEntityTransformed(oldEntity, newEntity, locationX, locationY));
        }

        public void OnEntitiesModified(List<MapModification> modifications)
        {
            ExceptionFreeAction(() => Callback.OnEntitiesModified(modifications));
        }

        public void OnKilled(int playerId, EntityTypes playerEntity, int locationX, int locationY)
        {
            ExceptionFreeAction(() => Callback.OnKilled(playerId, playerEntity, locationX, locationY));
        }

        public void OnGameDraw()
        {
            ExceptionFreeAction(() => Callback.OnGameDraw());
        }

        public void OnGameLost()
        {
            ExceptionFreeAction(() => Callback.OnGameLost());
        }

        public void OnGameWon(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnGameWon(playerId));
        }

        public void OnPing()
        {
            ExceptionFreeAction(() => Callback.OnPing());
        }

        #endregion
    }
}
