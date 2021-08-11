using System;
using System.Collections.Generic;
using System.Text;

namespace AsystentZOOM.VM.Common
{
    [Serializable]
    public abstract class SingletonBaseVM : BaseVM, IDisposable
    {
        public virtual void Dispose()
        {
        }
    }
}