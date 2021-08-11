using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JW
{
    [Serializable]
    public class SongInfo
    {
        public string SongName { get; set; }
        public string Text { get; set; }
        public string WebPage { get; set; }
        public string Description { get; set; }
        public string FileSource { get; set; }
        public int FileSize { get; set; }
        public string FileDestination { get; set; }
        public bool SaveOnDisk { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }

    public class MusicProgramDownloader : BaseDownloader
    {
        public async Task<List<SongInfo>> Download(VideoQualityEnum videoQuality, string path = null, bool downloadAll = false)
        {
            string baseAddress = "https://www.jw.org";
            string address = $"{baseAddress}/pl/biblioteka/muzyka-pieśni/piosenki";
            string videoQualityString = Enum.GetName(typeof(VideoQualityEnum), videoQuality).Replace("p", string.Empty) + "p";

            List<SongInfo> songList;
            int currentPosition = 0;
            string taskName = "Pobieranie listy piosenek";
            using (var webClient = new WebClient())
            {
                string songListHtml = await webClient.DownloadStringTaskAsync(new Uri(address));
                var songListHtmlDoc = new HtmlDocument();
                songListHtmlDoc.LoadHtml(songListHtml);
                songList = songListHtmlDoc.DocumentNode
                    .GetHtmlNode((n) => n.Name == "article")
                    .GetHtmlNodes(n => n.Name == "div" && n.HasClass("syn-body"))
                    .Select(x => new
                    {
                        ElementA = x.GetHtmlNode(n => n.Name == "h2").GetHtmlNode(n => n.Name == "a"),
                        ElementP = x.GetHtmlNode(n => n.Name == "p")
                    })
                    .Select(x => new SongInfo
                    {
                        SongName = x.ElementA.InnerText.Replace("\r", string.Empty).Trim(),
                        WebPage = baseAddress + x.ElementA.Attributes["href"].Value,
                        Description = x.ElementP.InnerText
                    })
                    .ToList();

                taskName = "Tworzenie listy niepobranych piosenek";
                currentPosition = 0;
                foreach (SongInfo song in songList)
                {
                    string songHtml = await webClient.DownloadStringTaskAsync(song.WebPage);
                    var songHtmlDoc = new HtmlDocument();
                    songHtmlDoc.LoadHtml(songHtml);
                    song.Text = songHtmlDoc.DocumentNode.GetHtmlNode(n => n.Name == "ol" && n.HasClass("source")).InnerText;
                    string dataJsonUrl = songHtmlDoc.DocumentNode.GetHtmlNode(n => n.HasClass("jsIncludeVideo")).Attributes["data-jsonurl"].Value;
                    Stream dataJson = await webClient.OpenReadTaskAsync(dataJsonUrl);
                    JsonDocument jsonDocument = await JsonDocument.ParseAsync(dataJson);

                    JsonElement fileJsonElement = jsonDocument.RootElement
                        .GetProperty("files")
                        .GetProperty("P")
                        .GetProperty("MP4")
                        .EnumerateArray()
                        .FirstOrDefault(c => c.GetProperty("label").GetString() == videoQualityString);
                    
                    song.FileSource = fileJsonElement.GetProperty("file").GetProperty("url").GetString();
                    song.FileSize = fileJsonElement.GetProperty("filesize").GetInt32();
                    string modifiedDatetime = fileJsonElement.GetProperty("file").GetProperty("modifiedDatetime").GetString();
                    song.ModifiedDateTime = DateTime.Parse(modifiedDatetime.Substring(0, 10), CultureInfo.InvariantCulture);

                    string fileName = song.FileSource.Split('/').Last();
                    fileName = !string.IsNullOrEmpty(path) ? Path.Combine(path, fileName) : fileName;
                    song.FileDestination = fileName;
                    song.SaveOnDisk = 
                        !new FileInfo(song.FileDestination).Exists || 
                         new FileInfo(song.FileDestination).Length != song.FileSize ||
                         downloadAll;

                    Interlocked.Increment(ref currentPosition);
                }
            }

            taskName = "Pobieranie piosenek na dysk";
            songList = songList.Where(s => s.SaveOnDisk).ToList();
            currentPosition = 0;
            Parallel.ForEach(songList, new ParallelOptions { MaxDegreeOfParallelism = 5 }, (song) =>
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(song.FileSource, song.FileDestination);
                }
                Interlocked.Increment(ref currentPosition);
            });
            return songList;
        }
    }
}