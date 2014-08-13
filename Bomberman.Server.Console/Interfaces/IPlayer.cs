using System;
using System.Collections.Generic;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console.Interfaces
{
    public enum PlayerStates
    {
        Connected,  // -> Playing
        Playing,    // ->  Dying|Winner
        Dying,      // -> Dead  intermediate state used when a player dies to avoid warning multiple time the same death if multiple bomb are exploding at the same time on the same player for example. Player is set to Dying, then when explosion handling is done, player is set to Died
        Winner,     // -> Connected
        Dead        // -> Connected
    }

    public delegate void ConnectionLostHandler(IPlayer player);

    public interface IPlayer : IBombermanCallback
    {
        string Name { get; }

        PlayerStates State { get; set; }

        int LocationX { get; set; }
        int LocationY { get; set; }

        int BombRange { get; set; }
        int BombCount { get; set; }
        int MaxBombCount { get; set; }
        List<EntityTypes> Bonuses { get; set; }

        EntityTypes PlayerEntity { get; set; }

        IBombermanCallback Callback { get; }

        event ConnectionLostHandler OnConnectionLost;

        // Heartbeat management
        DateTime LastActionToClient { get; } // used to check if heartbeat is needed

        // Timeout management
        DateTime LastActionFromClient { get; }
        int TimeoutCount { get; }

        void ResetTimeout();
        void SetTimeout();
    }
}
