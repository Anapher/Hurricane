using System;
using System.Windows.Input;

namespace Hurricane.ViewModelBase
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default return value for the CanExecute method is 'true'.
    /// </summary>
    /// <remarks></remarks>
    public class RelayCommand : ICommand
    {
        #region "Declarations"
        public delegate void ExecuteDelegate(object parameter);
        private readonly Func<bool> _canExecute;
        #endregion
        private readonly ExecuteDelegate _execute;

        #region "Constructors"
        public RelayCommand(ExecuteDelegate execute)
            : this(execute, null)
        {
        }

        public RelayCommand(ExecuteDelegate execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region "ICommand"
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

            //This is the RaiseEvent block

        }
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            else
            {
                return _canExecute.Invoke();
            }
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
        #endregion
    }
}
