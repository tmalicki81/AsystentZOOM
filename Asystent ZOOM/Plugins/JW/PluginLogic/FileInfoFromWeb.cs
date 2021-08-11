using System;

namespace JW
{
    public class FileInfoFromWeb
    {
        public event EventHandler OnCompletted;
        public void CallOnCompletted() => OnCompletted?.Invoke(this, EventArgs.Empty);

        public bool IsCompletted { get; set; } = true;
        public int Lp { get; set; }
        public string Title { get; set; }
        public string Checksum { get; set; }
        public string FileSource { get; set; }
        public int FileSize { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}