using System.Collections.Generic;
using Bomberman.Common.Contracts;

namespace Bomberman.Server.Interfaces
{
    public interface IPlayerManager
    {
        bool IsValid(string name);
        bool Exists(string name);

        int Add(IPlayer player);
        void Remove(IPlayer player);
        void Clear();

        int MaxPlayers { get; }
        int PlayerCount { get; }
        object LockObject { get; }

        IEnumerable<IPlayer> Players { get; }

        int GetId(IPlayer player);

        IPlayer this[string name] { get; }
        IPlayer this[int index] { get; }
        IPlayer this[IBombermanCallback callback] { get; } // Callback property from IPlayer should only be used here
    }
}
