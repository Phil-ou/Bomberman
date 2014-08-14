using Bomberman.Client.Interfaces;
using Bomberman.Client.WPF.MVVM;
using Bomberman.Common.Helpers;

namespace Bomberman.Client.WPF.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public delegate void ClientChangedEventHandler(IClient oldClient, IClient newClient);

        public event ClientChangedEventHandler ClientChanged;

        private IClient _client;
        public IClient Client
        {
            get { return _client; }
            set
            {
                if (_client != value)
                {
                    if (_client != null)
                        UnsubscribeFromClientEvents(_client);
                    IClient oldValue = _client;
                    _client = value;
                    ClientChanged.Do(x => x(oldValue, _client));
                    if (_client != null)
                        SubscribeToClientEvents(_client);
                }
            }
        }

        protected abstract void UnsubscribeFromClientEvents(IClient oldClient);
        protected abstract void SubscribeToClientEvents(IClient newClient);
    }
}
