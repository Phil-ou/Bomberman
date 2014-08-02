using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ClientWPF.Helpers
{
    public static class ExecuteOnUIThread
    {
        private static Dispatcher _uiDispatcher;
        public static TaskScheduler TaskScheduler { get; private set; }

        public static void Initialize()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            TaskScheduler = TaskScheduler.Current;
        }

        public static void Invoke(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            try
            {
                _uiDispatcher.Invoke(action, priority);
            }
            catch (Exception ex)
            {
                // TODO
                //Log.WriteLine(Log.LogLevels.Error, "Exception raised in ExecuteOnUIThread. {0}", ex);
            }
        }

        public static void InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            try
            {
                _uiDispatcher.InvokeAsync(action, priority);
            }
            catch (Exception ex)
            {
                // TODO
                //Log.WriteLine(Log.LogLevels.Error, "Exception raised in ExecuteOnUIThread. {0}", ex);
            }
        }
    }
}
