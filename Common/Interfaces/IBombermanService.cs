using System;
using System.ServiceModel;
using Common.DataContract;

namespace Common.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IBombermanCallbackService))]
    public interface IBombermanService
    {
        [OperationContract(IsOneWay = true)]
        void ConnectUser(string login);

        [OperationContract(IsOneWay = true)]
        void StartGame();

        [OperationContract(IsOneWay = true)]
        void MovePlayerToLocation(string login, ActionType actionType); //up,down,left,right
    }
}
