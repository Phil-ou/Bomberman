using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Bomberman.Client.Interfaces;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;
using Bomberman.Common.Helpers;

namespace Bomberman.Client.WCFProxy
{
    public class WCFProxy : IProxy
    {
        private DuplexChannelFactory<IBomberman> _factory;
        private readonly IBomberman _proxy;

        public WCFProxy(IBombermanCallback callback, string address)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (address == null)
                throw new ArgumentNullException("address");

            LastActionToServer = DateTime.Now;

            EndpointAddress endpointAddress = new EndpointAddress(address);

            // Create WCF proxy from endpoint
            Log.WriteLine(Log.LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callback);
            _factory = new DuplexChannelFactory<IBomberman>(instanceContext, binding, endpointAddress);
            _proxy = _factory.CreateChannel(instanceContext);
        }

        private void ExceptionFreeAction(Action action, [CallerMemberName] string actionName = null)
        {
            try
            {
                action();
                LastActionToServer = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLostHandler.Do(x => x());
                Disconnect();
            }
        }

        #region IProxy

        public DateTime LastActionToServer { get; private set; }
        public event ProxyConnectionLostDelegate ConnectionLostHandler;

        public bool Disconnect()
        {
            if (_factory == null)
                return false; // should connect first
            try
            {
                _factory.Close();
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Exception:{0}", ex);
                _factory.Abort();
            }
            _factory = null;
            return true;
        }

        #region IBomberman

        public void Login(string playerName)
        {
            ExceptionFreeAction(() => _proxy.Login(playerName));
        }

        public void Logout()
        {
            ExceptionFreeAction(() => _proxy.Logout());
        }

        public void StartGame(int mapId)
        {
            ExceptionFreeAction(() => _proxy.StartGame(mapId));
        }

        public void Move(Directions direction)
        {
            ExceptionFreeAction(() => _proxy.Move(direction));
        }

        public void PlaceBomb()
        {
            ExceptionFreeAction(() => _proxy.PlaceBomb());
        }

        public void Chat(string msg)
        {
            ExceptionFreeAction(() => _proxy.Chat(msg));
        }

        public void Heartbeat()
        {
            ExceptionFreeAction(() => _proxy.Heartbeat());
        }

        #endregion

        #endregion
    }
}
