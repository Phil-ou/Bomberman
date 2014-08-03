using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Server.Console
{
    public class Server
    {
        private readonly WCFHost _host;
        private readonly IPlayerManager _playerManager;

        public Server(WCFHost host, IPlayerManager playerManager)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");

            _host = host;
            _playerManager = playerManager;
        }

        public void Start()
        {
            _host.Start();
        }

        public void Stop()
        {
            _host.Stop();
        }
    }
}
