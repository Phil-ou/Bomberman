using Bomberman.Common.Contracts;

namespace Bomberman.Server.Console
{
    public interface IPlayer : IBombermanCallback
    {
        string Name { get; }
        int LocationX { get; set; }
        int LocationY { get; set; }

        IBombermanCallback Callback { get; }
    }
}
