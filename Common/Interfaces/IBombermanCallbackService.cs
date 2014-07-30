using System.Collections.Generic;
using System.ServiceModel;
using Common.DataContract;


namespace Common.Interfaces
{

    public interface IBombermanCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void OnUserConnected(string userName, List<string> logins, bool isCreator,bool canStartGame);

        [OperationContract(IsOneWay = true)]
        void OnGameStarted(Game newGame, string currentPlayerLogin);

        [OperationContract(IsOneWay = true)]
        void OnMove(LivingObject objectToMoveBefore, LivingObject objectToMoveAfter);
    }
}
