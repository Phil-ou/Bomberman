using System;
using System.ServiceModel;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class WCFHost : IBomberman
    {
        private ServiceHost _serviceHost;

        private readonly IPlayerManager _playerManager;
        private readonly Func<string, IBombermanCallback, IPlayer> _createPlayerFunc;

        public string Port { get; private set; }

        public WCFHost(string port, IPlayerManager playerManager, Func<string, IBombermanCallback, IPlayer> createPlayerFunc)
        {
            if (port == null)
                throw new ArgumentNullException("port");
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");
            if (createPlayerFunc == null)
                throw new ArgumentNullException("createPlayerFunc");

            Port = port;
            _playerManager = playerManager;
            _createPlayerFunc = createPlayerFunc;
        }

        public void Start()
        {
            Uri baseAddress = new Uri("net.tcp://localhost:" + Port);

            _serviceHost = new ServiceHost(this, baseAddress);
            _serviceHost.AddServiceEndpoint(typeof(IBomberman), new NetTcpBinding(SecurityMode.None), "/Bomberman");
            _serviceHost.Open();

            foreach (var endpt in _serviceHost.Description.Endpoints)
            {
                Log.WriteLine(Log.LogLevels.Info, "Enpoint address:\t{0}", endpt.Address);
                Log.WriteLine(Log.LogLevels.Info, "Enpoint binding:\t{0}", endpt.Binding);
                Log.WriteLine(Log.LogLevels.Info, "Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
            }
        }

        public void Stop()
        {
            // Close service host
            _serviceHost.Close();
        }

        #region IBomberman

        public void Login(string playerName)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Login {0}", playerName);

            LoginResults result = LoginResults.Successful;
            IPlayer player = null;
            int id = -1;
            lock (_playerManager.LockObject)
            {
                if (!_playerManager.IsValid(playerName))
                {
                    result = LoginResults.FailedInvalidName;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because name is invalid", playerName);
                }
                else if (_playerManager.Exists(playerName))
                {
                    result = LoginResults.FailedDuplicateName;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because it already exists", playerName);
                }
                else if (_playerManager.PlayerCount >= _playerManager.MaxPlayers)
                {
                    result = LoginResults.FailedTooManyPlayers;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because too many players are already connected", playerName);
                }
                else
                {
                    player = _createPlayerFunc(playerName, Callback);
                    //
                    id = _playerManager.Add(player);
                }
            }
            if (id >= 0 && player != null && result == LoginResults.Successful)
            {
                player.OnLogin(result, id, null); // TODO:
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Info, "Register failed for player {0}", playerName);
                //
                Callback.OnLogin(result, -1, null);
            }
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public void StartGame(int mapId)
        {
            throw new NotImplementedException();
        }

        public void Move(Directions direction)
        {
            throw new NotImplementedException();
        }

        public void PlaceBomb()
        {
            throw new NotImplementedException();
        }

        public void Chat(string msg)
        {
            throw new NotImplementedException();
        }

        public void Heartbeat()
        {
            throw new NotImplementedException();
        }

        #endregion

        private IBombermanCallback Callback
        {
            get
            {
                //MessageProperties messageProperties = OperationContext.Current.IncomingMessageProperties;
                //RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                //if (endpointProperty != null)
                //{
                //    IPAddress address = IPAddress.Parse(endpointProperty.Address);
                //}
                return OperationContext.Current.GetCallbackChannel<IBombermanCallback>();
            }
        }
    }
}
