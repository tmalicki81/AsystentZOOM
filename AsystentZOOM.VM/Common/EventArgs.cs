using System;

namespace AsystentZOOM.VM.Common
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value) => Value = value;
        public T Value { get; }
    }
}
