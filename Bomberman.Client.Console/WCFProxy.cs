using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Bomberman.Common;
using Bomberman.Common.Contracts;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Console
{
    // TODO: wrap every call to proxy by an exception handler
    public class WCFProxy : IBomberman
    {
        private DuplexChannelFactory<IBomberman> _factory;
        private readonly IBomberman _proxy;

        public WCFProxy(IBombermanCallback callback, string address)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (address == null)
                throw new ArgumentNullException("address");

            EndpointAddress endpointAddress = new EndpointAddress(address);

            // Create WCF proxy from endpoint
            Log.WriteLine(Log.LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callback);
            //_proxy = DuplexChannelFactory<IWCFTetriNET>.CreateChannel(instanceContext, binding, endpointAddress);
            _factory = new DuplexChannelFactory<IBomberman>(instanceContext, binding, endpointAddress);
            _proxy = _factory.CreateChannel(instanceContext);
        }

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
            _proxy.Login(playerName);
        }

        public void Logout()
        {
            _proxy.Logout();
        }

        public void StartGame(int mapId)
        {
            _proxy.StartGame(mapId);
        }

        public void Move(Directions direction)
        {
            _proxy.Move(direction);
        }

        public void PlaceBomb()
        {
            _proxy.PlaceBomb();
        }

        public void Chat(string msg)
        {
            _proxy.Chat(msg);
        }

        public void Heartbeat()
        {
            _proxy.Heartbeat();
        }

        #endregion
    }
}
