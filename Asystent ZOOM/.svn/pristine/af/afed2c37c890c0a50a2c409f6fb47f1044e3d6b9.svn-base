using System;
using System.IO;
using System.Text;

namespace BinaryBackupper
{
    public static class Log 
    {
        private static readonly object _locker = new object();
        public static void WriteLine(string text)
            => Write(text + Environment.NewLine);

        public static void Write(string text)
        {
            lock (_locker)
            {
                File.AppendAllText("BuildInfo\\BuildLog.txt", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}    {text}", Encoding.UTF8);
            }
        }
    }
}
