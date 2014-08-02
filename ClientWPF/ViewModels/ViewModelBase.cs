using System;
using System.Threading.Tasks;
using ClientWPF.MVVM;

namespace ClientWPF.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        protected void ExecuteOnUIThread(Action action)
        {
            Helpers.ExecuteOnUIThread.Invoke(action);
        }

        protected Task<T> ExecuteAsync<T>(Func<T> operation, Action<T> callback)
        {
            Task<T> task = Task<T>.Factory.StartNew(operation);
            if (callback != null)
                task.ContinueWith(t =>
                    {
                        switch (t.Status)
                        {
                                // Handle any exceptions to prevent UnobservedTaskException.
                            case TaskStatus.Faulted:
                                throw new ApplicationException("Problem with task (Status = Faulted)");
                                /* Error-handling logic */
                            case TaskStatus.RanToCompletion:
                                callback(t.Result);
                                break;
                        }
                    },
                                  Helpers.ExecuteOnUIThread.TaskScheduler); // Execute callback on UI thread
            return task;
        }

        protected Task ExecuteAsync(Action operation, Action callback)
        {
            return ExecuteAsync<bool>(
                () =>
                    {
                        operation();
                        return true;
                    },
                result =>
                    {
                        if (callback != null)
                            callback();
                    });
        }

        //protected void SendErrorMessage(Exception exception)
        //{
        //    SendErrorMessage(exception.Message, exception.ToString());
        //}

        //protected void SendErrorMessage(string title)
        //{
        //    SendErrorMessage(title, "");
        //}

        //protected void SendErrorMessage(ServiceStatus status)
        //{
        //    SendErrorMessage(status.OperationStatus.ToString(), status.Message);
        //}

        //protected void SendErrorMessage(string title, string detail)
        //{
        //    // TPA : tracetool test.
        //    TTrace.Error.Send(title, detail);
        //    AsyncMessenger.Send(new LogMessage
        //        {
        //            IsError = true,
        //            Title = title,
        //            Detail = detail
        //        });
        //}

        //protected void SendLogMessage(string title, string detail)
        //{
        //    // TPA : tracetool test.
        //    TTrace.Debug.Send(title, detail);

        //    // will be catched by LogViewModel
        //    AsyncMessenger.Send(new LogMessage
        //        {
        //            IsError = false,
        //            Title = title,
        //            Detail = detail
        //        });
        //}

        //protected void IsLoading(bool isLoading)
        //{
        //    AsyncMessenger.Send(new IsLoadingMessage
        //        {
        //            IsLoading = isLoading
        //        });
        //}
    }
}
