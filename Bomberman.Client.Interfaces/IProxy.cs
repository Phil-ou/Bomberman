using System;
using Bomberman.Common.Contracts;

namespace Bomberman.Client.Interfaces
{
    public delegate void ProxyConnectionLostDelegate();

    public interface IProxy : IBomberman
    {
        DateTime LastActionToServer { get; } // used to check if heartbeat is needed

        event ProxyConnectionLostDelegate ConnectionLostHandler;

        bool Disconnect();
    }
}
