using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsystentZOOM.VM.Common
{
    public class SingletonInstanceHelper : IDisposable
    {
        private static List<Type> _types = new List<Type>();
        private static object _locker = new object();

        private readonly Type _type;
        public SingletonInstanceHelper(Type t)
        {
            lock(_locker)
            {
                _type = t;
                _types.Add(_type);
            }
        }

        public static bool IsDeserializing(Type t)
        {
            return _types.Contains(t);
        }

        public void Dispose()
        {
            lock (_locker)
            {
                _types.Remove(_type);
            }
        }
    }
}
