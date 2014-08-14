using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Bomberman.Client.Interfaces;
using Bomberman.Client.WPF.ViewModels.Login;
using Bomberman.Client.WPF.ViewModels.Play;
using Bomberman.Client.WPF.ViewModels.WaitRoom;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public LoginViewModel LoginViewModel { get; protected set; }
        public WaitRoomViewModel WaitRoomViewModel { get; protected set; }
        public PlayViewModel PlayViewModel { get; protected set; }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (Set(() => IsConnected, ref _isConnected, value))
                {
                    OnPropertyChanged("IsDisconnected");
                    OnPropertyChanged("IsConnectedAndGameStopped");
                }
            }
        }

        private bool _isGameStarted;
        public bool IsGameStarted
        {
            get { return _isGameStarted; }
            set
            {
                if (Set(() => IsGameStarted, ref _isGameStarted, value))
                    OnPropertyChanged("IsGameStopped");
            }
        }

        public MainViewModel()
        {
            //
            bool isDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            //
            LoginViewModel = new LoginViewModel();
            WaitRoomViewModel = new WaitRoomViewModel();
            PlayViewModel = new PlayViewModel();

            //
            IsConnected = false;
            IsGameStarted = false;

            //
            ClientChanged += OnClientChanged;

            //
            if (!isDesignMode)
            {
                // Create client
                Client = new Client();
            }
        }

        #region ViewModelBase

        protected override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.LoggedOn -= OnLoggedOn;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.GameDraw -= OnGameDraw;
            oldClient.GameLost -= OnGameLost;
            oldClient.GameWon -= OnGameWon;
        }

        protected override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.LoggedOn += OnLoggedOn;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.GameStarted += OnGameStarted;
            newClient.GameDraw += OnGameDraw;
            newClient.GameLost += OnGameLost;
            newClient.GameWon += OnGameWon;
        }

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            // TODO: use reflection
            LoginViewModel.Client = newClient;
            WaitRoomViewModel.Client = newClient;
            PlayViewModel.Client = newClient;
        }

        #endregion

        #region IClient event handlers

        private void OnGameWon(bool won, string name)
        {
            IsGameStarted = false;
        }

        private void OnGameLost()
        {
            IsGameStarted = false;
        }

        private void OnGameDraw()
        {
            IsGameStarted = false;
        }

        private void OnGameStarted(Map map)
        {
            IsGameStarted = true;
        }

        private void OnConnectionLost()
        {
            IsConnected = false;
            IsGameStarted = false;
        }

        private void OnLoggedOn(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps, bool isGameStarted)
        {
            if (result == LoginResults.Successful)
                IsConnected = true;
            else
                IsConnected = false;
        }

        #endregion
    }

    public class MainViewModelDesignData : MainViewModel
    {
        public MainViewModelDesignData()
        {
            LoginViewModel = new LoginViewModelDesignData();
            WaitRoomViewModel = new WaitRoomViewModelDesignData();
            PlayViewModel = new PlayViewModelDesignData();
        }
    }
}
