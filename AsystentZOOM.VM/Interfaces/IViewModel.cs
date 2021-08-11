using AsystentZOOM.VM.Common;

namespace AsystentZOOM.VM.Interfaces
{
    public interface IViewModel<VM>
        where VM : BaseVM
    {
        VM ViewModel { get; }
    }
}
