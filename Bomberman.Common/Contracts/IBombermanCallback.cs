using System.Collections.Generic;
using System.ServiceModel;
using Bomberman.Common.DataContracts;

namespace Bomberman.Common.Contracts
{
    public interface IBombermanCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLogin(LoginResults result, int playerId, List<MapDescription> maps);

        [OperationContract(IsOneWay = true)]
        void OnUserConnected(string username, int playerId);

        [OperationContract(IsOneWay = true)]
        void OnUserDisconnected(int id);

        [OperationContract(IsOneWay = true)]
        void OnGameStarted(Location start, Map map);

        [OperationContract(IsOneWay = true)]
        void OnMoved(bool succeed, Location oldLocation, Location newLocation);

        [OperationContract(IsOneWay = true)]
        void OnBombPlaced();

        [OperationContract(IsOneWay = true)]
        void OnChatReceived(int playerId, string msg);

        [OperationContract(IsOneWay = true)]
        void OnEntityAdded(EntityTypes entity, Location location);

        [OperationContract(IsOneWay = true)]
        void OnEntityDeleted(EntityTypes entity, Location location);

        [OperationContract(IsOneWay = true)]
        void OnEntityMoved(EntityTypes entity, Location oldLocation, Location newLocation);

        [OperationContract(IsOneWay = true)]
        void OnKilled(int playerId);

        [OperationContract(IsOneWay = true)]
        void OnGameDraw();

        [OperationContract(IsOneWay = true)]
        void OnGameLost();

        [OperationContract(IsOneWay = true)]
        void OnGameWon(int playerId);

        [OperationContract(IsOneWay = true)]
        void OnPing();
    }
}
