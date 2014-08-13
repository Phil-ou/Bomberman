using System;
using Bomberman.Common.Contracts;

namespace Bomberman.Client.Interfaces
{
    public delegate void ProxyConnectionLostHandler();

    public interface IProxy : IBomberman
    {
        DateTime LastActionToServer { get; } // used to check if heartbeat is needed

        event ProxyConnectionLostHandler OnConnectionLost;

        bool Disconnect();
    }
}
