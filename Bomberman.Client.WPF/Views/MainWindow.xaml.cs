using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Bomberman.Client.WPF.ViewModels;

namespace Bomberman.Client.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Title = String.Format("Bomberman {0}.{1}", version.Major, version.Minor);

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _vm = new MainViewModel();
                DataContext = _vm;
            }

            Loaded += (sender, args) =>
            {
                Focusable = true;
                Focus();
                KeyDown += OnKeyDown;
                KeyUp += OnKeyUp;
            };
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Left)
                _vm.PlayViewModel.MoveLeft();
            if (keyEventArgs.Key == Key.Right)
                _vm.PlayViewModel.MoveRight();
            if (keyEventArgs.Key == Key.Up)
                _vm.PlayViewModel.MoveUp();
            if (keyEventArgs.Key == Key.Down)
                _vm.PlayViewModel.MoveDown();
            if (keyEventArgs.Key == Key.Space)
                _vm.PlayViewModel.PlaceBomb();
        }

        private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
        }
    }
}
