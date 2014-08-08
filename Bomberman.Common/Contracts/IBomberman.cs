using System.ServiceModel;
using Bomberman.Common.DataContracts;

namespace Bomberman.Common.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IBombermanCallback))]
    public interface IBomberman
    {
        [OperationContract(IsOneWay = true)]
        void Login(string playerName);

        [OperationContract(IsOneWay = true)]
        void Logout();

        [OperationContract(IsOneWay = true)]
        void StartGame(int mapId);

        [OperationContract(IsOneWay = true)]
        void Move(Directions direction);

        [OperationContract(IsOneWay = true)]
        void PlaceBomb();

        [OperationContract(IsOneWay = true)]
        void Chat(string msg);

        [OperationContract(IsOneWay = true)]
        void Heartbeat();
    }
}
