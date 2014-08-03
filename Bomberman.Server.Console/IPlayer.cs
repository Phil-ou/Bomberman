using Bomberman.Common.Contracts;

namespace Bomberman.Server.Console
{
    public interface IPlayer : IBombermanCallback
    {
        string Name { get; }

        IBombermanCallback Callback { get; }
    }
}
