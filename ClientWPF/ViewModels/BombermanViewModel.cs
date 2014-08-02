using ClientWPF.ViewModels.Login;
using Common.Interfaces;

namespace ClientWPF.ViewModels
{
    public class BombermanViewModel : ViewModelBase
    {
        #region Properties

        public IBombermanService Proxy { get; private set; }

        private LoginViewModel _loginViewModel;
        public LoginViewModel LoginViewModel
        {
            get { return _loginViewModel; }
            set { Set(() => LoginViewModel, ref _loginViewModel, value); }
        }

        #endregion 

        #region Methods

        public void Initialize()
        {
            LoginViewModel.Initialize(Proxy);
        }

        #endregion
    }

    public class BombermanViewModelDesignData : BombermanViewModel
    {
        public BombermanViewModelDesignData()
        {
            LoginViewModel = new LoginViewModelDesignData();
        }
    }
}
