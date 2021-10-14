using AsystentZOOM.VM.Interfaces;
using System;

namespace AsystentZOOM.VM.Common
{
    public class RelayCommand : IRelayCommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
            => _execute();

        public void Execute()
            => _execute();

        public bool CanExecute(object parameter)
            => _canExecute != null ? _canExecute() : true;

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : IRelayCommand<T>
    {
        private static bool AlwaysTrue(T obj) => true;

        public event EventHandler CanExecuteChanged;

        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? AlwaysTrue;
        }

        public void Execute(object parameter)
            => _execute((T)parameter);

        public bool CanExecute(object parameter)
            => _canExecute((T)parameter);

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
