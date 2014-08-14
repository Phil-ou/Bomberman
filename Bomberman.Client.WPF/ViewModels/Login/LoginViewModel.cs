using System.Collections.Generic;
using System.Windows.Input;
using Bomberman.Client.Interfaces;
using Bomberman.Client.WPF.MVVM;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.WPF.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(() => IsConnected, ref _isConnected, value); }
        }
        
        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        private string _address;
        public string Address
        {
            get { return _address; }
            set { Set(() => Address, ref _address, value); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { Set(() => Status, ref _status, value); }
        }
        
        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                _loginCommand = _loginCommand ?? new RelayCommand(Login);
                return _loginCommand;
            }
        }

        private ICommand _logoutCommand;
        public ICommand LogoutCommand
        {
            get
            {
                _logoutCommand = _logoutCommand ?? new RelayCommand(Logout);
                return _logoutCommand;
            }
        }

        public LoginViewModel()
        {
            // TODO: read from config
            Name = "SinaC";
            Address = "net.tcp://localhost:9999/Bomberman";
        }

        #region ViewModelBase

        protected override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.LoggedOn -= OnLoggedOn;
            oldClient.ConnectionLost -= OnConnectionLost;
        }

        protected override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.LoggedOn += OnLoggedOn;
            newClient.ConnectionLost += OnConnectionLost;
        }

        #endregion

        #region IClient event handlers

        private void OnLoggedOn(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps, bool isGameStarted)
        {
            if (result == LoginResults.Successful)
                IsConnected = true;
            else
                IsConnected = false;

            Status = result.ToString();
        }

        private void OnConnectionLost()
        {
            IsConnected = false;
        }
        
        #endregion

        private void Login()
        {
            // TODO: check name and address
            if (!Client.IsConnected)
                Client.Login(callback => new WCFProxy.WCFProxy(callback, Address), Name);
        }

        private void Logout()
        {
            if (Client.IsConnected)
            {
                IsConnected = false;
                Client.Logout();
            }
        }
    }

    public class LoginViewModelDesignData : LoginViewModel
    {
        public LoginViewModelDesignData()
        {
        }
    }
}
