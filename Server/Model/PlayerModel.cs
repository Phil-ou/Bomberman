using Common.Interfaces;

namespace Server.Model
{
    public class PlayerModel
    {
        public string Login { get; set; }

        public int CurrentScore { get; set; }

        //public int BestScore { get; set; }

        public bool IsCreator { get; set; }

        public IBombermanCallbackService CallbackService { get; set; } 
    }
}
