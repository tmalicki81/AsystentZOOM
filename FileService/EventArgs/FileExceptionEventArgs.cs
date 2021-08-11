using System;

namespace FileService.EventArgs
{
    public class FileExceptionEventArgs : System.EventArgs
    {
        public string FileName { get; internal set; }
        public Exception Exception { get; internal set; }
    }
}
