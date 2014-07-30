using System;
using System.Collections.Generic;
using System.Linq;
using Client.Logic;
using Common.DataContract;
using Common.Interfaces;
using Common.Log;

namespace Client
{
    //[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class BombermanCallbackService : IBombermanCallbackService
    {
        private static ClientProcessor ClientProcessor = new ClientProcessor();

        public void OnUserConnected(string login, List<String> loginsList, bool isCreator, bool canStartGame)
        {
            ClientProcessor.OnUserConnected(login,loginsList,isCreator,canStartGame);
        }

        public void OnGameStarted(Game newGame, string currentPlayerLogin)
        {
            ClientProcessor.OnGameStarted(newGame, currentPlayerLogin);
        }

        public void OnMove(LivingObject objectToMoveBefore, LivingObject objectToMoveAfter)
        {
            ClientProcessor.OnMove(objectToMoveBefore, objectToMoveAfter);
        }
    }
}
