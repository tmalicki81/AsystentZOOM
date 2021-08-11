using AsystentZOOM.VM.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AsystentZOOM.VM.Interfaces
{
    public interface ILayerOutputView
    {
    }

    public interface ILayerOutput<VM> : ILayerOutputView, IViewModel<VM>
        where VM : BaseVM
    {
    }
}
