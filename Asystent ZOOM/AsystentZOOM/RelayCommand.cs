using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AsystentZOOM
{
    public interface IAsyncCommand : ICommand
    {
        void ExecuteAsync();
        bool CanExecute();
    }

    public class AsyncCommand : IAsyncCommand
    {
        public event EventHandler CanExecuteChanged;

        private bool _isExecuting;
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public AsyncCommand(
            Action execute,
            Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute()
            => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public void ExecuteAsync()
        {
            if (CanExecute())
            {
                try
                {
                    _isExecuting = true;
                    var t = new Task(() =>
                    {
                        try
                        {
                            _execute();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            _isExecuting = false;
                        }
                    });
                    t.Start();
                }
                finally
                {
                    _isExecuting = false;
                }
            }
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        bool ICommand.CanExecute(object parameter)
            => CanExecute();

        void ICommand.Execute(object parameter)
            => ExecuteAsync();
    }

}
