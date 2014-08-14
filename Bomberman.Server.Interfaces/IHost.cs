using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Interfaces
{
    public delegate void LoginDelegate(IPlayer player, int playerId);
    public delegate void LogoutDelegate(IPlayer player);
    public delegate void StartGameDelegate(IPlayer player, int mapId);
    public delegate void MoveDelegate(IPlayer player, Directions direction);
    public delegate void PlaceBombDelegate(IPlayer player);
    public delegate void ChatDelegate(IPlayer player, string msg);

    public delegate void DisconnectPlayerDelegate(IPlayer player);

    public interface IHost : IBomberman
    {
        void Start();
        void Stop();

        event LoginDelegate LoginHandler;
        event LogoutDelegate LogoutHandler;
        event StartGameDelegate StartGameHandler;
        event MoveDelegate MoveHandler;
        event PlaceBombDelegate PlaceBombHandler;
        event ChatDelegate ChatHandler;

        event DisconnectPlayerDelegate PlayerDisconnectedHandler;
    }
}
