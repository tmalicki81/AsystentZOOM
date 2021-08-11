using System;
using System.Collections.Generic;
using System.Linq;

namespace AsystentZOOM.VM.Common
{
    public class EventAggregator
    {
        public class SubscribeInfo
        {
            public string MessageCode { get; set; }
            public Predicate<object> Predicate { get; set; }
            public Action<object> Action { get; set; }
            public Type ArgumentType { get; set; }
        }

        static EventAggregator()
        {
            _locker = new object();
            _subscribeInfoList = new List<SubscribeInfo>();
        }

        private static List<SubscribeInfo> _subscribeInfoList;
        private static readonly object _locker;

        public static void Publish<TArgs>(string messageCode, TArgs args) 
        {
            List<SubscribeInfo> matchedList = GetSubscriptions(messageCode);
            foreach (SubscribeInfo subscribeInfo in matchedList)
            {
                if(subscribeInfo == null || subscribeInfo.Predicate(args))
                    subscribeInfo.Action(args);
            }
        }

        public static void Publish(string messageCode)
        {
            List<SubscribeInfo> matchedList = GetSubscriptions(messageCode);
            foreach (SubscribeInfo subscribeInfo in matchedList)
            {
                if (subscribeInfo.ArgumentType != null)
                    throw new InvalidOperationException($"Nie podano argumentu typu {subscribeInfo.ArgumentType.Name}");
                subscribeInfo.Action(null);
            }
        }

        public static SubscribeInfo Subscribe(string messageCode, Action typedAction, Func<bool> typedPredicate)
        {
            Action<object> noTypedAction = new Action<object>((x) => typedAction());
            Predicate<object> noTypedPredicate = new Predicate<object>((x) => typedPredicate());
            return RegisterSubscription(messageCode, noTypedAction, noTypedPredicate);
        }

        private static SubscribeInfo RegisterSubscription(string messageCode, Action<object> noTypedAction, Predicate<object> predicate, Type argumentType = null)
        {
            lock (_locker)
            {
                var si = new SubscribeInfo
                {
                    MessageCode = messageCode,
                    Action = noTypedAction,
                    Predicate = predicate,
                    ArgumentType = argumentType
                };
                _subscribeInfoList.Add(si);
                return si;
            }
        }

        private static List<SubscribeInfo> GetSubscriptions(string messageCode)
        {
            lock (_locker)
            {
                return _subscribeInfoList.Where(x => x.MessageCode == messageCode).ToList();
            }
        }

        public static SubscribeInfo Subscribe<TArgs>(string messageCode, Action<TArgs> typedAction, Func<TArgs, bool> typedPredicate)
        {
            var noTypedAction = new Action<object>((x) => typedAction((TArgs)x));
            var noTypedPredicate = new Predicate<object>((x) => typedPredicate((TArgs)x));

            return RegisterSubscription(messageCode, noTypedAction, noTypedPredicate, typeof(TArgs));
        }

        public static void UnSubscribe(string messageCode)
        {
            lock (_locker)
            {
                _subscribeInfoList = _subscribeInfoList.Where(x => x.MessageCode != messageCode).ToList();
            }
        }
    }
}
