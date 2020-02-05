using System;
using System.Windows.Input;

namespace SpecialJobs.Helpers
{
    //public class DelegateCommand : ICommand
    //{
    //    private readonly Action _command;
    //    private readonly Func<bool> _canExecute;
    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public DelegateCommand(Action command, Func<bool> canExecute = null)
    //    {
    //        if (command == null)
    //            throw new ArgumentNullException();
    //        _canExecute = canExecute;
    //        _command = command;
    //    }

    //    public void Execute(object parameter)
    //    {
    //        _command();
    //    }

    //    public bool CanExecute(object parameter)
    //    {
    //        return _canExecute == null || _canExecute();
    //    }

    //}
    //класс для Execute с параметром и CanExecute без параметра
    public class DelegateCommandMy<T> : ICommand
    {
        private readonly Action<T> _command;
        private readonly Func<bool> _canExecute;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommandMy(Action<T> command, Func<bool> canExecute = null)
        {
            if (command == null)
                throw new ArgumentNullException();
            _canExecute = canExecute;
            _command = command;
        }

        public void Execute(object parameter)
        {
            _command((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

    }
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

      
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }
      
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion
    }

}