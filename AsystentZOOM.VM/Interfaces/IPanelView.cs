using AsystentZOOM.VM.Common;

namespace AsystentZOOM.VM.Interfaces
{
    public interface IPanelView
    {
        int PanelNr { get; }
        string PanelName { get; }
        void ChangeVisible(bool visible);
    }

    public interface IPanelView<VM> : IPanelView, IViewModel<VM>
        where VM : BaseVM
    {
    }
}
