using System;
using System.ServiceModel;
using Common.DataContract;
using Server.Logic;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class BombermanService : Common.Interfaces.IBombermanService
    {
        private static ServerProcessor ServerProcessor = new ServerProcessor();

        public static void StartServer()
        {
            ServerProcessor.StartServer();
        }

        public void ConnectUser(string login)
        {
            ServerProcessor.ConnectUser(login);
        }

        public void StartGame()
        {
            ServerProcessor.StartGame();
        }

        public void MovePlayerToLocation(string login, ActionType actionType)
        {
            ServerProcessor.MovePlayerToLocation(login, actionType);
        }
    }
}
