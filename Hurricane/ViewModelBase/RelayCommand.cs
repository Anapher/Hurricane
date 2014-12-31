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
        private readonly Func<bool> _CanExecute;
        #endregion
        private readonly ExecuteDelegate _Execute;

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
            _Execute = execute;
            _CanExecute = canExecute;
        }
        #endregion

        #region "ICommand"
        public event EventHandler CanExecuteChanged
        {

            add
            {
                if (_CanExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_CanExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            //This is the RaiseEvent block

        }
        public bool CanExecute(object parameter)
        {
            if (_CanExecute == null)
            {
                return true;
            }
            else
            {
                return _CanExecute.Invoke();
            }
        }

        public void Execute(object parameter)
        {
            _Execute.Invoke(parameter);
        }
        #endregion
    }
}
