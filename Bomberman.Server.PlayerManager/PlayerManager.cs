using System;
using System.Collections.Generic;
using System.Linq;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Server.Interfaces;

namespace Bomberman.Server.PlayerManager
{
    public class PlayerManager : IPlayerManager
    {
        private readonly IPlayer[] _players;

        public PlayerManager(int maxPlayers)
        {
            MaxPlayers = maxPlayers;
            _players = new IPlayer[maxPlayers];
            LockObject = new object();
        }

        #region IPlayerManager

        public bool IsValid(string name)
        {
            return true; // TODO: min 2 char, max 20 char, no special char, etc...
        }

        public bool Exists(string name)
        {
            return _players.Any(x => x != null && x.Name == name);
        }

        public int Add(IPlayer player)
        {
            bool alreadyExists = _players.Any(x => x != null && (x == player || x.Name == player.Name));
            if (!alreadyExists)
            {
                // insert in first empty slot
                for (int i = 0; i < MaxPlayers; i++)
                    if (_players[i] == null)
                    {
                        _players[i] = player;
                        return i;
                    }
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "{0} already registered", player.Name);
            return -1;
        }

        public void Remove(IPlayer player)
        {
            for (int i = 0; i < MaxPlayers; i++)
                if (_players[i] == player)
                {
                    _players[i] = null;
                    return;
                }
        }

        public void Clear()
        {
            for (int i = 0; i < MaxPlayers; i++)
                _players[i] = null;
        }

        public int MaxPlayers { get; private set; }

        public int PlayerCount
        {
            get { return _players.Count(x => x != null); }
        }

        public object LockObject { get; private set; }

        public IEnumerable<IPlayer> Players
        {
            get { return _players.Where(x => x != null); }
        }

        public int GetId(IPlayer player)
        {
            return player == null ? -1 : Array.IndexOf(_players, player);
        }

        public IPlayer this[string name]
        {
            get { return _players.FirstOrDefault(x => x != null && x.Name == name); }
        }

        public IPlayer this[int index]
        {
            get
            {
                if (index >= MaxPlayers)
                    return null;
                return _players[index];
            }
        }

        public IPlayer this[IBombermanCallback callback]
        {
            get { return _players.FirstOrDefault(x => x != null && x.Callback == callback); }
        }

        #endregion
    }
}
