using System.ComponentModel;
using ClientWPF.Helpers;
using ClientWPF.ViewModels;

namespace ClientWPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Bomberman
    {
        public Bomberman()
        {
            ExecuteOnUIThread.Initialize();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                BombermanViewModel vm = new BombermanViewModel();
                DataContext = vm;
                vm.Initialize();
            }
        }
    }
}
