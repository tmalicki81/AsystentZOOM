using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;

namespace AsystentZOOM.VM.Common
{
    public static class IsolatedStorageHelper
    {
        private static string GetFileName(Type t) 
            =>  $"{t.Name}.xml";

        private static readonly object _locker = new object();

        public static void OpenObject<T>() 
        {
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                PropertyInfo pi = typeof(IsolatedStorageFile).GetProperty("RootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
                string rootDirectory = (string)pi.GetValue(file);
                string fullPath = Path.Combine(rootDirectory, GetFileName(typeof(T)));
                if (!File.Exists(fullPath))
                {
                    var newObject = (T)Activator.CreateInstance(typeof(T));
                    SaveObject(newObject);
                }
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = fullPath
                };
                Process.Start(psi);
            }
        }

        public static T LoadObject<T>() 
        {
            bool mustDeleteFile = false;
            using (new SingletonInstanceHelper(typeof(T)))
            {
                string path = GetFileName(typeof(T));
                using (var file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!file.FileExists(path))
                        return Activator.CreateInstance<T>();
                    using (var stream = file.OpenFile(path, FileMode.Open, FileAccess.Read))
                    {
                        var serializer = new CustomXmlSerializer(typeof(T));
                        try
                        {
                            stream.Position = 0;
                            return (T)serializer.Deserialize(stream);
                        }
                        catch
                        {
                            mustDeleteFile = true;
                        }
                    }
                    if (mustDeleteFile)
                    {
                        file.DeleteFile(path);
                    }
                    return Activator.CreateInstance<T>();
                }
            }
        }
        
        public static void SaveObject<T>(T obj)
        {
            string path = GetFileName(obj.GetType());
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (_locker)
                {
                    if (file.FileExists(path))
                        file.DeleteFile(path);
                    using (var stream = file.OpenFile(path, FileMode.Create, FileAccess.Write))
                    {
                        var serializer = new CustomXmlSerializer(obj.GetType());
                        serializer.Serialize(stream, obj);
                    }
                }
            }
        }

        public static void DeleteObject(Type objectType)
        {
            string path = GetFileName(objectType);
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (_locker)
                {
                    if (file.FileExists(path))
                        file.DeleteFile(path);
                }
            }
        }
    }    
}
