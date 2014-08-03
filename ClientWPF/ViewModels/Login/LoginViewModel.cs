using System;
using System.Windows.Input;
using ClientWPF.MVVM;
using Common.Interfaces;

namespace ClientWPF.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        #region Properties

        public IBombermanService Proxy { get; private set; }

        public string Login { get; set; }

        private ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                _connectCommand = _connectCommand ?? new RelayCommand(Connect);
                return _connectCommand;
            }
        }

        #endregion

        #region Methods

        private void Connect()
        {
            int id = Guid.NewGuid().GetHashCode();
            Proxy.ConnectUser(Login, id);
        }

        public void Initialize(IBombermanService proxy)
        {
            Proxy = proxy;
        }

        #endregion
    }

    public class LoginViewModelDesignData : LoginViewModel
    {
        public LoginViewModelDesignData()
        {
            Login = "Test";
        }
    }
}
