using System.Collections.Generic;
using System.ServiceModel;
using Common.DataContract;


namespace Common.Interfaces
{
    
    public interface IBombermanCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void OnUserConnected(Player player, List<string> logins, bool canStartGame);

        [OperationContract(IsOneWay = true)]
        void OnGameStarted(Game newGame);

        [OperationContract(IsOneWay = true)]
        void OnMove(LivingObject objectToMoveBefore, LivingObject objectToMoveAfter);
    }
}
