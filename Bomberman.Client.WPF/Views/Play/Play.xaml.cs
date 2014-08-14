using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bomberman.Client.WPF.Helpers;
using Bomberman.Client.WPF.ViewModels.Play;
using Bomberman.Common.Helpers;

namespace Bomberman.Client.WPF.Views.Play
{
    /// <summary>
    /// Interaction logic for Play.xaml
    /// </summary>
    public partial class Play : UserControl
    {
        //private readonly CancellationTokenSource _cancellationTokenSource;
        //private readonly Task _keyboardTask;

        public Play()
        {
            InitializeComponent();

            //_cancellationTokenSource = new CancellationTokenSource();
            //_keyboardTask = Task.Factory.StartNew(KeyboardTask, _cancellationTokenSource.Token);
        }

        // Best method but it work when a key is hit in any application =D

        //private void KeyboardTask()
        //{
        //    while (true)
        //    {
        //        if (_cancellationTokenSource.IsCancellationRequested)
        //            break;
                
        //        ExecuteOnUIThread.Invoke(KeyboardAction);

        //        bool isSignaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(33);
        //        if (isSignaled)
        //            break;
        //    }
        //}

        //private void KeyboardAction()
        //{
        //    if (Keyboard.IsKeyDown(Key.Up))
        //        Up();
        //    if (Keyboard.IsKeyDown(Key.Down))
        //        Down();
        //    if (Keyboard.IsKeyDown(Key.Left))
        //        Left();
        //    if (Keyboard.IsKeyDown(Key.Right))
        //        Right();
        //    if (Keyboard.IsKeyDown(Key.Space))
        //        Space();
        //}

        //private void Up()
        //{
        //    PlayViewModel vm = DataContext as PlayViewModel;
        //    vm.Do(x => x.MoveUp());
        //}

        //private void Down()
        //{
        //    PlayViewModel vm = DataContext as PlayViewModel;
        //    vm.Do(x => x.MoveDown());
        //}

        //private void Left()
        //{
        //    PlayViewModel vm = DataContext as PlayViewModel;
        //    vm.Do(x => x.MoveLeft());
        //}

        //private void Right()
        //{
        //    PlayViewModel vm = DataContext as PlayViewModel;
        //    vm.Do(x => x.MoveRight());
        //}

        //private void Space()
        //{
        //    PlayViewModel vm = DataContext as PlayViewModel;
        //    vm.Do(x => x.PlaceBomb());
        //}
    }
}
