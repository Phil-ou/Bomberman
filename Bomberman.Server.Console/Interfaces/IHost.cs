using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console.Interfaces
{
    public delegate void LoginHandler(IPlayer player, int playerId);
    public delegate void LogoutHandler(IPlayer player);
    public delegate void StartGameHandler(IPlayer player, int mapId);
    public delegate void ChangeDirectionHandler(IPlayer player, Directions direction);
    public delegate void MoveHandler(IPlayer player);
    public delegate void PlaceBombHandler(IPlayer player);
    public delegate void ChatHandler(IPlayer player, string msg);

    public interface IHost : IBomberman
    {
        void Start();
        void Stop();

        event LoginHandler OnLogin;
        event LogoutHandler OnLogout;
        event StartGameHandler OnStartGame;
        event ChangeDirectionHandler OnChangeDirection;
        event MoveHandler OnMove;
        event PlaceBombHandler OnPlaceBomb;
        event ChatHandler OnChat;
    }
}
