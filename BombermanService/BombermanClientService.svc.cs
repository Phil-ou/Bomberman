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
    public class BombermanClientService : IBombermanClientService
    {
        public Game CreateNewGame(Player creatorPlayer, string gameName, int mapNumber)
        {
            throw new NotImplementedException();
        }
    }
}
