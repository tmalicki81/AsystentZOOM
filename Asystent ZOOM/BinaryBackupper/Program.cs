using BinaryHelper;
using BinaryHelper.FileRepositories;
using FileService.EventArgs;
using System;
using System.IO;
using System.Linq;

namespace BinaryBackupper
{
    class Program
    {
        static void Main(string[] args)
        {
            bool mustSendToServer;
            try
            {
                if (args.Any(x => x == "-increment"))
                {
                    // Inkrementacja
                }
                else
                {
                    Directory.SetCurrentDirectory(@"C:\Users\tmali\source\repos\AsystentZOOM\AsystentZOOM.GUI");
                }
                Log.WriteLine("Katalog z binariami: " + Directory.GetCurrentDirectory());
                BuildVersionHelper.Increment(out mustSendToServer);
            }
            catch (Exception ex)
            {
                Log.WriteLine("Inkrementacja: "+ex.ToString());
                throw;
            }

            if (!mustSendToServer)
            {
                Log.WriteLine($"Pominięto wysyłanie binariów na serwer.");
            }
            else
            {
                var bin = new LocalBinFileRepository();
                var ftp = new FtpBinFileRepository();
                bin.OnPushedFile += Bin_OnPushed;
                try
                {
                    bin.PushToTarget(targetFileRepository: ftp, null);
                }
                catch (Exception ex)
                {
                    Log.WriteLine("PushToTarget: " + ex.ToString());
                    throw;
                }
            }
        }

        private static void Bin_OnPushed(object sender, PushedFileEventArgs e)
        {
            Log.WriteLine($"Skopiowano: {(e.Copied ? "Tak" : "Nie")}, Kopia zapasowa: {(e.IsBackup ? "Tak" : "Nie")} Postęp: {e.PercentCompletted} %   Plik: {e.FileName}");
        }
    }
}