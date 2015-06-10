using System;
using System.Windows.Input;

namespace Hurricane.ViewModel
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default return value for the CanExecute method is 'true'.
    /// </summary>
    /// <remarks></remarks>
    public class RelayCommand : ICommand
    {
        public delegate void ExecuteDelegate(object parameter);
        private readonly Func<bool> _canExecute;
        private readonly ExecuteDelegate _execute;

        public RelayCommand(ExecuteDelegate execute)
            : this(execute, null)
        {
        }

        public RelayCommand(ExecuteDelegate execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {

            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
    }
}
