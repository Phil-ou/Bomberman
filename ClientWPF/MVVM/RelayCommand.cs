using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ClientWPF.MVVM
{
    public class RelayCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            var val = parameter;

            if (parameter != null && parameter.GetType() != typeof(T) && parameter is IConvertible)
                val = Convert.ChangeType(parameter, typeof(T), null);

            if (CanExecute(val) && _execute != null)
            {
                if (val == null)
                {
                    if (typeof (T).IsValueType)
                        _execute(default(T));
                    else
                        _execute((T) val);
                }
                else
                    _execute((T) val);
            }
        }

        #endregion // ICommand Members
    }

    public class RelayCommand : ICommand
    {
        readonly Action _execute;
        readonly Func<bool> _canExecute;

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter) // parameter ignored
        {
            return _canExecute == null || _canExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) // parameter ignored
        {
            _execute();
        }

        #endregion // ICommand Members
    }
}
