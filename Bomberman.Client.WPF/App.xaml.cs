using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using Bomberman.Client.WPF.Helpers;
using Bomberman.Common;

namespace Bomberman.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ExecuteOnUIThread.Initialize();

            // Initialize Log
            string logFilename = "WPF_" + Guid.NewGuid().ToString().Substring(0, 5) + ".log";
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);

            //
            base.OnStartup(e);
        }
    }
}
