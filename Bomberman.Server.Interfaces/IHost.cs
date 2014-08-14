using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Interfaces
{
    public delegate void LoginEventHandler(IPlayer player, int playerId);
    public delegate void LogoutEventHandler(IPlayer player);
    public delegate void StartGameEventHandler(IPlayer player, int mapId);
    public delegate void MoveEventHandler(IPlayer player, Directions direction);
    public delegate void PlaceBombEventHandler(IPlayer player);
    public delegate void ChatEventHandler(IPlayer player, string msg);

    public delegate void DisconnectPlayerEventHandler(IPlayer player);

    public interface IHost : IBomberman
    {
        void Start();
        void Stop();

        event LoginEventHandler HostLogin;
        event LogoutEventHandler HostLogout;
        event StartGameEventHandler HostStartGame;
        event MoveEventHandler HostMove;
        event PlaceBombEventHandler HostPlaceBomb;
        event ChatEventHandler HostChat;

        event DisconnectPlayerEventHandler PlayerDisconnected;
    }
}
