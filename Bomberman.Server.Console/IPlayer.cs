using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console
{
    public interface IPlayer : IBombermanCallback
    {
        string Name { get; }
        Location Location { get; }

        IBombermanCallback Callback { get; }
    }
}
