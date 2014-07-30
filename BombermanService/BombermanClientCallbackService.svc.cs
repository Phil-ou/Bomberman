using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Common.DataContract;

namespace BombermanService
{
    public 

    public class BombermanClientCallBackService : IBombermanClientCallbackService
    {
        public void GameCreated(Game createdGame)
        {
            Console.WriteLine("game created : " + createdGame.GameName);
        }
    }
}
