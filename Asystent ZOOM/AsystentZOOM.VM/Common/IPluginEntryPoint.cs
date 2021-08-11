using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace AsystentZOOM.VM.Common
{
    public interface IPluginEntryPoint
    {
        void Execute(MainVM mainVM, ItemCollection miAddins);
    }
}
