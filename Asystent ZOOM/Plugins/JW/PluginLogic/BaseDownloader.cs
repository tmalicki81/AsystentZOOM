using AsystentZOOM.VM.ViewModel;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace JW
{
    public abstract class BaseDownloader
    {
        protected const string BaseAddress = "https://www.jw.org";

        protected List<FileInfoFromWeb> GetJpgFileInfoListFromArticle(string hrefArticle) 
        {
            if (string.IsNullOrWhiteSpace(hrefArticle))
                return new List<FileInfoFromWeb>();

            using (var webClient = new WebClient())
            {
                string lll = webClient.DownloadString(hrefArticle);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(lll);
                return htmlDoc.DocumentNode
                    .GetHtmlNodes(n => n.Name == "figure")
                    .Select(x => x.GetHtmlNode(n => n.Name == "span"))
                    .Select(n => new FileInfoFromWeb
                    {
                        Lp = n.LinePosition,
                        Title = n.Attributes["data-img-att-alt"].Value,
                        FileSource = n.Attributes["data-zoom"].Value
                    })
                    .ToList();
            }
        }

        protected List<FileInfoFromWeb> GetMp4FileInfoListFromArticle(string pointTitle, string hrefArticle, VideoQualityEnum videoQuality = VideoQualityEnum.p480)
        {
            if (string.IsNullOrWhiteSpace(hrefArticle))
                return new List<FileInfoFromWeb>();

            var list = new List<FileInfoFromWeb>();
            using (var webClient = new WebClient())
            {
                string lll = webClient.DownloadString(hrefArticle);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(lll);
                List<string> hrefToVideoList = htmlDoc.DocumentNode
                    .GetHtmlNodes(n => 
                        n.Name == "a" && 
                        (
                            n.HasClass("pub-nwtsv") || 
                            n.ParentNode.InnerText.Contains("OBEJRZYJ FILM") ||
                            n.ParentNode.InnerText.Contains("Odtwórz film")
                        ))
                    .Select(n => n.Attributes["href"]?.Value)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(v => v.StartsWith(BaseAddress) ? v : BaseAddress + v)
                    .ToList();

                hrefToVideoList.Add(hrefArticle);

                foreach (string item in hrefToVideoList)
                {
                    var mp4FileInfo = GetMp4FileInfoFromArticle(pointTitle, item, videoQuality);
                    if(mp4FileInfo != null)
                        list.Add(mp4FileInfo);
                }
                return list;
            }
        }

        protected List<System.Windows.Forms.Form> _formList = new List<System.Windows.Forms.Form>();

        protected FileInfoFromWeb GetMp4FileInfoFromArticle(string pointTitle, string hrefArticle, VideoQualityEnum videoQuality = VideoQualityEnum.p480)
        {
            string videoQualityString = Enum.GetName(typeof(VideoQualityEnum), videoQuality).Replace("p", string.Empty) + "p";

            JsonDocument jsonDocument;
            using (var webClient = new WebClient())
            {
                string html = webClient.DownloadString(hrefArticle);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                string dataJsonUrl = htmlDoc.DocumentNode.GetHtmlNode(n => n.HasClass("jsIncludeVideo"))?.Attributes["data-jsonurl"]?.Value;
                if (string.IsNullOrEmpty(dataJsonUrl))
                {
                    var newFileInfoFromWeb = new FileInfoFromWeb { IsCompletted = false };

                    var webBrowser = MainVM.Dispatcher.Invoke(() => new System.Windows.Forms.WebBrowser());
                    webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
                    var webBrowserForm = new System.Windows.Forms.Form
                    {
                        WindowState = System.Windows.Forms.FormWindowState.Minimized,
                        ShowInTaskbar = false,
                        Location = new System.Drawing.Point(-1000, -1000), 
                        Text = pointTitle
                    };
                    _formList.Add(webBrowserForm);
                    webBrowserForm.Controls.Add(webBrowser);
                    webBrowser.DocumentCompleted += (s, e) =>
                    {
                        newFileInfoFromWeb.Lp = -1;
                        if (e.Url.AbsolutePath == "blank")
                            return;

                        html = webBrowser.Document.Body.InnerHtml;
                        htmlDoc.LoadHtml(html);
                        var aaa = htmlDoc.DocumentNode.GetHtmlNode(n => 
                            n.Name == "a" && 
                            n.ParentNode.HasClass("dropdownBody") &&
                            n.Attributes["href"].Value.ToUpper().EndsWith($"{videoQualityString}.mp4".ToUpper()));

                        if (aaa != null)
                        {
                            webBrowser.Dispose();
                            _formList.Remove(webBrowserForm);
                            webBrowserForm.Close();

                            newFileInfoFromWeb.FileSource = aaa.Attributes["href"].Value;
                            newFileInfoFromWeb.IsCompletted = true;
                            newFileInfoFromWeb.CallOnCompletted();
                        }                        
                    };

                    App.Current.Dispatcher.Invoke(() => webBrowserForm.Show());

                    webBrowser.Url = new Uri("https://www.jw.org/pl/");
                    webBrowser.Navigate(webBrowser.Url);
                    webBrowser.Navigate(hrefArticle);
                    webBrowserForm.Invoke(new Action(webBrowserForm.Hide));

                    return newFileInfoFromWeb;
                }
                using (Stream dataJson = webClient.OpenRead(dataJsonUrl))
                {
                    jsonDocument = JsonDocument.Parse(dataJson);
                }
            }
            JsonElement fileJsonElement = jsonDocument.RootElement
                .GetProperty("files")
                .GetProperty("P")
                .GetProperty("MP4")
                .EnumerateArray()
                .FirstOrDefault(c => c.GetProperty("label").GetString() == videoQualityString);

            string modifiedDatetime = fileJsonElement.GetProperty("file").GetProperty("modifiedDatetime").GetString();
            var file = new FileInfoFromWeb
            {
                Title = fileJsonElement.GetProperty("title").GetString(),
                FileSource = fileJsonElement.GetProperty("file").GetProperty("url").GetString(),
                Checksum = fileJsonElement.GetProperty("file").GetProperty("checksum").GetString(),
                FileSize = fileJsonElement.GetProperty("filesize").GetInt32(),
                ModifiedTime = DateTime.Parse(modifiedDatetime.Substring(0, 10), CultureInfo.InvariantCulture)
            };
            return file; 
        }
    }
}