using Common.DataContract;
using Common.Interfaces;

namespace Server.Model
{
    public class PlayerModel
    {
        public Player Player { get; set; }

        public IBombermanCallbackService CallbackService { get; set; } 
    }
}
