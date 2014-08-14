using Bomberman.Client.Interfaces;

namespace Bomberman.Client
{
    public class Opponent : IOpponent
    {
        #region IOpponent

        public int Id { get; set; }
        public string Name { get; set; }

        #endregion
    }
}
