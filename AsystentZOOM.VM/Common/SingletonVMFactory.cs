using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Common
{
    public static class SingletonVMFactory
    {
        private static readonly object _layersLocker = new object();
        private static List<ILayerVM> _layers;
        public static List<ILayerVM> Layers
        {
            get
            {
                lock (_layersLocker)
                {
                    if (_layers == null)
                    {
                        var result = new List<ILayerVM>();
                        var properties = typeof(SingletonVMFactory)
                            .GetProperties(BindingFlags.Static | BindingFlags.Public)
                            .Where(t => t.PropertyType.GetInterfaces().Any(i => i == typeof(ILayerVM)));
                        foreach (var p in properties)
                            result.Add((ILayerVM)p.GetValue(null));
                        _layers = result;
                    }
                    return _layers;
                }
            }
        }

        private static bool IsBaseType(Type type, Type baseType)
        {
            var currentType = type;
            while (currentType.BaseType != null)
            {
                currentType = currentType.BaseType;
                if (currentType == baseType)
                    return true;
            }
            return false;
        }

        private static readonly object _allSingletonsLocker = new object();
        private static List<SingletonBaseVM> _allSingletons;
        public static List<SingletonBaseVM> AllSingletons
        {
            get
            {
                lock (_allSingletonsLocker)
                {
                    if (_allSingletons == null)
                    {
                        var result = new List<SingletonBaseVM>();
                        var properties = typeof(SingletonVMFactory)
                            .GetProperties(BindingFlags.Static | BindingFlags.Public)
                            .Where(t => IsBaseType(t.PropertyType, typeof(SingletonBaseVM)));
                        foreach (var p in properties)
                            result.Add((SingletonBaseVM)p.GetValue(null));
                        _allSingletons = result;
                    }
                    return _allSingletons;
                }
            }
        }

        private static readonly object _setSingletonValuesLocker = new object();
        public static SingletonBaseVM SetSingletonValues(object source)
        {
            lock (_setSingletonValuesLocker)
            {
                Type layerType = source.GetType();
                SingletonBaseVM target = AllSingletons.First(x => x.GetType() == layerType);
                target.IsDataReady = false;

                var properties = layerType
                    .GetProperties()
                    .Where(p => p.SetMethod != null && p.GetMethod != null)
                    .Where(p => !p.GetCustomAttributes(typeof(XmlIgnoreAttribute), false).Any())
                    .ToList();
                foreach (var pi in properties)
                {
                    object sourcePropertyValue = pi.GetValue(source);
                    pi.SetValue(target, sourcePropertyValue);
                }
                target.OnDeserialized(null);
                target.IsDataReady = true;
                return target;
            }
        }

        private static bool EqualsVM(BaseVM source, BaseVM target)
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();
            
            if (sourceType != targetType)
                return false;

            var sourceSerializer = new XmlSerializer(sourceType);
            var targetSerializer = new XmlSerializer(targetType);
            using (var newValueWriter = new StringWriter())
            using (var oldValueWriter = new StringWriter())
            {
                sourceSerializer.Serialize(newValueWriter, source);
                targetSerializer.Serialize(oldValueWriter, target);
                newValueWriter.Flush();
                oldValueWriter.Flush();
                return (newValueWriter.ToString() == oldValueWriter.ToString());
            }
        }

        private static bool EqualsCollections(IEnumerable source, IEnumerable target)
        {
            var serializer = new XmlSerializer(source.GetType());
            using (var newValueWriter = new StringWriter())
            using (var oldValueWriter = new StringWriter())
            {
                serializer.Serialize(newValueWriter, source);
                serializer.Serialize(oldValueWriter, target);
                newValueWriter.Flush();
                oldValueWriter.Flush();
                return (newValueWriter.ToString() == oldValueWriter.ToString());
            }
        }

        public static bool CopyValuesWhenDifferent<T>(T source, ref T target)
            where T : BaseVM
        {
            if (source == null)
            {
                target = null;
                return false;
            }
            Type sourceType = source?.GetType();
            Type targetType = target?.GetType();

            if (target == null || targetType != sourceType)
            {
                target = (T)Activator.CreateInstance(sourceType);
                targetType = target.GetType();
            }
            if (EqualsVM(source, target))
                return false;

            var properties = sourceType.GetProperties()
                .Where(p => p.SetMethod != null && p.GetMethod != null)
                .Where(p => !p.GetCustomAttributes(typeof(XmlIgnoreAttribute), false).Any())
                .ToList();

            source.IsDataReady = false;
            target.IsDataReady = false;

            foreach (var pi in properties)
            {
                object sourcePropertyValue = pi.GetValue(source);
                object targetPropertyValue = pi.GetValue(target);

                // Kolekcje obiektów
                if (sourcePropertyValue is IEnumerable<BaseVM> sourceEnumerable &&
                    targetPropertyValue is IEnumerable<BaseVM> targetEnumerable)
                {
                    // Jeśli kolekcja lub jej elementy uległy zmianie
                    if (!EqualsCollections(sourceEnumerable, targetEnumerable))
                    {
                        var sourceList = sourceEnumerable as IList;
                        var targetList = targetEnumerable as IList;

                        // Usuń elementy z oryginału, których nie ma 
                        IList toRemoveFromTarget = targetEnumerable
                            .Where(s => !sourceEnumerable.Any(t => s.InstanceId == t.InstanceId))
                            .ToList();
                        foreach (BaseVM itemToRemove in toRemoveFromTarget)
                        {
                            targetList.Remove(itemToRemove);
                        }

                        // Zmień wartości pozycji lub dodaj nowe pozycje do listy
                        int sourceItemIndex = 0;
                        foreach (BaseVM sourceItem in sourceList)
                        {
                            if (targetList.Count - 1 >= sourceItemIndex &&
                                targetList[sourceItemIndex] is BaseVM targetItem2 &&
                                sourceItem.InstanceId == targetItem2?.InstanceId)
                            {
                                CopyValuesWhenDifferent(sourceItem, ref targetItem2);
                            }
                            else if (targetEnumerable.FirstOrDefault(t => t.InstanceId == sourceItem.InstanceId) is BaseVM targetItem &&
                                     targetItem != null)
                            {
                                CopyValuesWhenDifferent(sourceItem, ref targetItem);

                                // TODO: Dopracować
                                targetList.Remove(targetItem);
                                targetList.Insert(sourceItemIndex, targetItem);
                            }
                            else
                            {
                                sourceItem.OnDeserialized(null);
                                targetList.Insert(sourceItemIndex, sourceItem);
                            }
                            sourceItemIndex++;
                        }
                    }
                }

                // Kolekcje wartości skalarnych
                else if (sourcePropertyValue is IEnumerable &&
                         targetPropertyValue is IEnumerable)
                {
                    pi.SetValue(target, sourcePropertyValue);
                }

                // Wartości obiektowe
                else if (sourcePropertyValue is BaseVM sourcePropertyVM &&
                         targetPropertyValue is BaseVM targetPropertyVM)
                {
                    if (sourcePropertyVM.InstanceId == targetPropertyVM.InstanceId)
                        // Jeśli jest to modyfikacja obiektu => zmień wartości właściwości
                        CopyValuesWhenDifferent(sourcePropertyVM, ref targetPropertyVM);
                    else
                        // Jeśli jest to nowy obiekt => Kopiuj wszystko
                        pi.SetValue(target, sourcePropertyValue);
                }

                // Wartości skalarne
                else
                {
                    if (sourcePropertyValue == null && targetPropertyValue != null ||
                        sourcePropertyValue != null && targetPropertyValue == null ||
                        sourcePropertyValue != null && !sourcePropertyValue.Equals(targetPropertyValue))
                    {
                        // Jeśli wartość jest inna niż oryginalna
                        pi.SetValue(target, sourcePropertyValue);
                    }
                }

                // Wywołaj
                (targetPropertyValue as IXmlDeserializationCallback)?.OnDeserialized(null);
            }
            target.OnDeserialized(null);
            source.IsDataReady = true;
            target.IsDataReady = true;
            return true;
        }

        public static void SaveAllSingletons()
            => AllSingletons.ForEach(s => IsolatedStorageHelper.SaveObject(s));

        public static void DisposeAllSingletons()
            => AllSingletons.ForEach(s => s.Dispose());

        private static MainVM _main;
        public static MainVM Main
            => _main ??= IsolatedStorageHelper.LoadObject<MainVM>();

        private static BackgroundVM _background;
        public static BackgroundVM Background
            => _background ??= IsolatedStorageHelper.LoadObject<BackgroundVM>();

        private static TimePieceVM _timePiece;
        public static TimePieceVM TimePiece
            => _timePiece ??= IsolatedStorageHelper.LoadObject<TimePieceVM>();

        public static MeetingVM Meeting 
            => _meeting ??= new MeetingVM();
        private static MeetingVM _meeting;

        private static VideoVM _video;
        public static VideoVM Video
            => _video ??= IsolatedStorageHelper.LoadObject<VideoVM>();

        private static AudioVM _audio;
        public static AudioVM Audio
            => _audio ??= IsolatedStorageHelper.LoadObject<AudioVM>();

        private static ImageVM _image;
        public static ImageVM Image
            => _image ??= IsolatedStorageHelper.LoadObject<ImageVM>();
    }
}
