using AsystentZOOM.VM.Common;

namespace AsystentZOOM.Plugins.JW.Common
{
    public class KvpVM<TKey, TValue> : BaseVM
    {
        private TKey _key;
        public TKey Key
        {
            get => _key;
            set => SetValue(ref _key, value, nameof(Key));
        }

        private TValue _value;
        public TValue Value
        {
            get => _value;
            set => SetValue(ref _value, value, nameof(Value));
        }
    }
}
