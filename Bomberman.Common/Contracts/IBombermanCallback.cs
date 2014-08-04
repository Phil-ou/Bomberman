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
        void OnGameStarted(int locationX, int locationY, Map map);

        [OperationContract(IsOneWay = true)]
        void OnMoved(bool succeed, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY);

        [OperationContract(IsOneWay = true)]
        void OnBombPlaced();

        [OperationContract(IsOneWay = true)]
        void OnChatReceived(int playerId, string msg);

        [OperationContract(IsOneWay = true)]
        void OnEntityAdded(EntityTypes entity, int locationX, int locationY);

        [OperationContract(IsOneWay = true)]
        void OnEntityDeleted(EntityTypes entity, int locationX, int locationY);

        [OperationContract(IsOneWay = true)]
        void OnEntityMoved(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY);

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
