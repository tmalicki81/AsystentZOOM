using System.Windows.Input;

namespace AsystentZOOM.VM.Interfaces
{
    public interface IRelayCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

    public interface IRelayCommand<T> : IRelayCommand
    {
    }
}
