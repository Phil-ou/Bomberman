﻿using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Interfaces
{
    public delegate void LoginHandler(IPlayer player, int playerId);
    public delegate void LogoutHandler(IPlayer player);
    public delegate void StartGameHandler(IPlayer player, int mapId);
    public delegate void MoveHandler(IPlayer player, Directions direction);
    public delegate void PlaceBombHandler(IPlayer player);
    public delegate void ChatHandler(IPlayer player, string msg);

    public delegate void DisconnectPlayerHandler(IPlayer player);

    public interface IHost : IBomberman
    {
        void Start();
        void Stop();

        event LoginHandler OnLogin;
        event LogoutHandler OnLogout;
        event StartGameHandler OnStartGame;
        event MoveHandler OnMove;
        event PlaceBombHandler OnPlaceBomb;
        event ChatHandler OnChat;

        event DisconnectPlayerHandler OnPlayerDisconnected;
    }
}
