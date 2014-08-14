using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Bomberman.Client.WPF.Helpers;
using Bomberman.Client.WPF.MVVM;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.WPF.ViewModels.WaitRoom
{
    public class WaitRoomViewModel : ViewModelBase
    {
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

        public bool IsGameStopped
        {
            get { return !IsGameStarted; }
        }
        
        private ObservableCollection<string> _players;
        public ObservableCollection<string> Players
        {
            get { return _players; }
            set { Set(() => Players, ref _players, value); }
        }

        private List<MapDescription> _maps;
        public List<MapDescription> Maps
        {
            get { return _maps; }
            set { Set(() => Maps, ref _maps, value); }
        }

        private MapDescription _selectedMap;
        public MapDescription SelectedMap
        {
            get { return _selectedMap; }
            set { Set(() => SelectedMap, ref _selectedMap, value); }
        }

        private ObservableCollection<string> _chats;
        public ObservableCollection<string> Chats
        {
            get { return _chats; }
            set { Set(() => Chats, ref _chats, value); }
        }

        private ICommand _startGameCommand;
        public ICommand StartGameCommand
        {
            get
            {
                _startGameCommand = _startGameCommand ?? new RelayCommand(StartGame);
                return _startGameCommand;
            }
        }

        public WaitRoomViewModel()
        {
            Players = new ObservableCollection<string>();
            Chats = new ObservableCollection<string>();
        }

        #region ViewModelBase

        protected override void UnsubscribeFromClientEvents(Interfaces.IClient oldClient)
        {
            oldClient.LoggedOn -= OnLoggedOn;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.UserConnected -= OnUserConnected;
            oldClient.UserDisconnected -= OnUserDisconnected;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.GameDraw -= OnGameDraw;
            oldClient.GameLost -= OnGameLost;
            oldClient.GameWon -= OnGameWon;
            oldClient.Killed -= OnKilled;
            oldClient.ChatReceived -= OnChatReceived;
        }

        protected override void SubscribeToClientEvents(Interfaces.IClient newClient)
        {
            newClient.LoggedOn += OnLoggedOn;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.UserConnected += OnUserConnected;
            newClient.UserDisconnected += OnUserDisconnected;
            newClient.GameStarted += OnGameStarted;
            newClient.GameDraw += OnGameDraw;
            newClient.GameLost += OnGameLost;
            newClient.GameWon += OnGameWon;
            newClient.Killed += OnKilled;
            newClient.ChatReceived += OnChatReceived;
        }

        #endregion

        #region IClient event handlers

        private void OnChatReceived(int playerId, string player, string msg)
        {
            string line;
            if (playerId == Client.Id)
                line = String.Format("You: {0}", msg);
            else
                line = String.Format("{0}: {1}", player, msg);
            AddChatLine(line);
        }

        private void OnGameWon(bool won, string name)
        {
            string line;
            if (won)
                line = "You have WON";
            else
                line = String.Format("{0} has WON", name);
            AddChatLine(line);
            IsGameStarted = false;
        }

        private void OnKilled(string name)
        {
            string line = String.Format("{0} has been KILLED", name);
            AddChatLine(line);
        }

        private void OnGameLost()
        {
            string line = "You have LOST";
            AddChatLine(line);
            IsGameStarted = false;
        }

        private void OnGameDraw()
        {
            string line = "Game ended in a DRAW";
            AddChatLine(line);
            IsGameStarted = false;
        }

        private void OnGameStarted(Map map)
        {
            string line = String.Format("Game started using map:{0}", map.Description.Title);
            AddChatLine(line);
            IsGameStarted = true;
        }

        private void OnUserDisconnected(string player, int playerId)
        {
            string line = String.Format("{0} has disconnected", player);
            AddChatLine(line);
            RemovePlayer(player);
        }

        private void OnUserConnected(string player, int playerId)
        {
            string line = String.Format("{0} has connected", player);
            AddChatLine(line);
            AddPlayer(player);
        }

        private void OnConnectionLost()
        {
            string line = "Connection LOST";
            AddChatLine(line);
            IsGameStarted = false;
        }

        private void OnLoggedOn(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps, bool isGameStarted)
        {
            string line;
            if (result == LoginResults.Successful)
            {
                ClearPlayers();
                AddPlayer(Client.Name);
                Maps = maps;
                if (Maps.Any())
                    SelectedMap = Maps.FirstOrDefault(x => x.Title.ToLower().Contains("simple"));
                line = "Logon successfull";
            }
            else
                line = String.Format("Failed to logon : {0}", result);
            AddChatLine(line);
            IsGameStarted = isGameStarted;
        }

        #endregion

        private void AddChatLine(string msg)
        {
            ExecuteOnUIThread.Invoke(() => Chats.Add(msg));
        }

        private void ClearPlayers()
        {
            ExecuteOnUIThread.Invoke(() => Players.Clear());
        }

        private void AddPlayer(string player)
        {
            ExecuteOnUIThread.Invoke(() => Players.Add(player));
        }

        private void RemovePlayer(string player)
        {
            ExecuteOnUIThread.Invoke(() => Players.Remove(player));
        }

        private void StartGame()
        {
            if (Client.IsConnected && !Client.IsPlaying && SelectedMap != null)
                Client.StartGame(SelectedMap.Id);
        }
    }

    public class WaitRoomViewModelDesignData : WaitRoomViewModel
    {
        public WaitRoomViewModelDesignData()
        {
            Chats = new ObservableCollection<string>
            {
                "Logon successfull",
                "KVDF has connected",
            };
            Players = new ObservableCollection<string>
            {
                "SinaC",
                "KVDF",
            };
            Maps = new List<MapDescription>
            {
                new MapDescription
                {
                    Id = 0,
                    Size = 10,
                    Title = "Small map",
                    Description = "Bla bla bla bla bla bla bla"
                },
                new MapDescription
                {
                    Id = 1,
                    Size = 20,
                    Title = "Medium map",
                    Description = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                }
            };
        }
    }
}
