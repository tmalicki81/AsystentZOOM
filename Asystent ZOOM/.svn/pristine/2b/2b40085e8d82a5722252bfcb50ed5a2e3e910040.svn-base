using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileService.Common
{
    public static class StreamHelper
    {
        public static byte[] ToByteArray(Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
                return ms.ToArray();
            }
        }

        public static MemoryStream GetMemoryStream(Stream stream)
            => new MemoryStream(ToByteArray(stream));

        public static MemoryStream GetMemoryStream(string fileName)
            => new MemoryStream(File.ReadAllBytes(fileName));
    }
}
