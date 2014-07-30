using System.Collections.Generic;
using Common.DataContract;

namespace Server.Model
{
    public class ServerModel
    {
        public ServerStatus ServerStatus { get; set; }

        public List<PlayerModel> PlayersOnline { get; set; }

        public Game GameCreated { get; set; }

        public void Initialize()
        {
            PlayersOnline = new List<PlayerModel>();
            ServerStatus = ServerStatus.Started;
        }
    }

    public enum ServerStatus
    {
        Started,
        Stopped
    }
}
