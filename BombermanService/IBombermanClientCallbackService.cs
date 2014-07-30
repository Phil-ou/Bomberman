
using System.ServiceModel;
using Common.DataContract;

namespace BombermanService
{
    public interface IBombermanClientCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void GameCreated(Game createdGame);
    }
}
