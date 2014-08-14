using System;
using System.Reflection;
using System.Windows;

namespace Bomberman.Client.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Title = String.Format("Bomberman {0}.{1}", version.Major, version.Minor);
        }
    }
}
