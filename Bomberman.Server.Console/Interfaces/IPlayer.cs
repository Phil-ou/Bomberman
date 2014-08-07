using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console.Interfaces
{
    public enum PlayerStates
    {
        Connected,  // -> Playing
        Playing,    // ->  Dying|Winner
        Dying,      // -> Dead
        Winner,     // -> Connected
        Dead        // -> Connected
    }

    public interface IPlayer : IBombermanCallback
    {
        string Name { get; }

        PlayerStates State { get; set; }

        Directions Direction { get; set; }
        int LocationX { get; set; }
        int LocationY { get; set; }

        EntityTypes PlayerEntity { get; set; }

        IBombermanCallback Callback { get; }
    }
}
