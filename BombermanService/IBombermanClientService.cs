using System.ServiceModel;
using Common.DataContract;

namespace BombermanService
{
    [ServiceContract(CallbackContract = typeof(IBombermanClientCallbackService))]
    public interface IBombermanClientService
    {
        [OperationContract]
        Game CreateNewGame(Player creatorPlayer, string gameName, int mapNumber);
    }
}
