using AsystentZOOM.Plugins.JW.Common;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.FileRepositories;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.ViewModel;
using FileService.Common;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JW
{
    public class WednesdayMeetingsDownloader : BaseDownloader
    {
        private readonly static Dictionary<int, string> ddd = new Dictionary<int, string>
        {
            { 1, "styczen"      },
            { 2, "luty"         },
            { 3, "marzec"       },
            { 4, "kwiecien"     },
            { 5, "maj"          },
            { 6, "czerwiec"     },
            { 7, "lipiec"       },
            { 8, "sierpien"     },
            { 9, "wrzesien"     },
            { 10, "pazdziernik" },
            { 11, "listopad"    },
            { 12, "grudzien"    },
        };

        private readonly static Dictionary<int, string> ccc = new Dictionary<int, string>
        {
            { 1, "stycznia"         },
            { 2, "lutego"           },
            { 3, "marca"            },
            { 4, "kwietnia"         },
            { 5, "maja"             },
            { 6, "czerwca"          },
            { 7, "lipca"            },
            { 8, "sierpnia"         },
            { 9, "września"         },
            { 10, "pazdziernika"    },
            { 11, "listopada"       },
            { 12, "grudnia"         },
        };

        private static string GetAddress(DateTime date)
        {
            int firstMonthInPeriod = date.Month % 2 == 1
                ? date.Month
                : date.Month - 1;
            int lastMonthInPeriod = firstMonthInPeriod + 1;

            var sb = new StringBuilder("https://www.jw.org/pl/biblioteka/program-zebran-swiadkow-jehowy/")
                .Append($"{ddd[firstMonthInPeriod]}-{ddd[lastMonthInPeriod]}-{date.Year}-mwb/")
                .Append("Chrze%C5%9Bcija%C5%84skie-%C5%BCycie-i-s%C5%82u%C5%BCba-");

            var ps = WeekHelper.GetWeeksInMonth(date.AddDays(-7))
                .Union(WeekHelper.GetWeeksInMonth(date))
                .Distinct()
                .ToList();

            var (monday, sunday) = ps.Where(x => x.monday <= date).Last();
            if (monday.Month < sunday.Month)
                sb.Append($"{monday.Day}-{ccc[monday.Month]}-do-{sunday.Day}-{ccc[sunday.Month]}");
            else
                sb.Append($"{monday.Day}-{sunday.Day}-{ccc[monday.Month]}");
            sb.Append($"-{date.Year}/");

            return sb.ToString();
        }

        public MeetingVM CreateMeeting(DateTime dateTime, string chairman, string host, string coHost, List<string> persons)
        {
            var meeting = new MeetingVM();
            meeting.WebAddress = GetAddress(dateTime);

            string meetingPointListHtml;
            using (var webClient = new WebClient())
            {
                meetingPointListHtml = webClient.DownloadString(new Uri(meeting.WebAddress));
            }
            var meetingPointListHtmlDoc = new HtmlDocument();
            meetingPointListHtmlDoc.LoadHtml(meetingPointListHtml);

            HtmlNode articleHtmlNode = meetingPointListHtmlDoc.DocumentNode.GetHtmlNode((n) => n.Name == "article");

            // Meeting title
            meeting.MeetingTitle = "Zebranie w tygodniu " + articleHtmlNode
                .GetHtmlNode((n) => n.Name == "header")
                .GetHtmlNode((n) => n.Name == "h1")
                .InnerText;

            // Meeting source
            string meetingSource = articleHtmlNode
                .GetHtmlNode((n) => n.Name == "header")
                .GetHtmlNode((n) => n.Name == "h2")
                .GetHtmlNodes((n) => n.Name == "a")
                .Select(z => z.GetHtmlNode((n) => n.Name == "a"))
                .Select(z => z.GetHtmlNode((n) => n.Name == "strong").InnerText)
                .Aggregate((a, b) => a + " " + b);

            HtmlNode bodyTxtNode = articleHtmlNode.GetHtmlNode(n => n.Name == "div" && n.HasClass("bodyTxt"));

            // Songs list
            var songs = bodyTxtNode
                .GetHtmlNodes(n => n.HasClass("pub-sjj"))
                .Select(n => BaseAddress + n.Attributes["href"].Value)
                .Select(s => GetMp4FileInfoFromArticle("Pieśń", s))
                .ToList();

            AddParameters(meeting.ParameterList, new Dictionary<string, string> 
            {
                { "Źródło",         meetingSource },
                { PersonsRoles.Przewodniczacy, chairman      },
                { PersonsRoles.Host,           host          },
                { PersonsRoles.Porzadkowy,     coHost        },
                { "Pieśni",         songs.Select(s => s.Title.Split('.').First()).Aggregate((a, b) => a + ",  " + b) }
            });

            CreateMeetingPoints(meeting, bodyTxtNode, true);
            
            var firstPoint = meeting.MeetingPointList.First();
            AddSong(firstPoint, chairman, songs.First());

            // Dodaj zegar
            var timePieceVM = new TimePieceVM
            {
                AlertMinTime = TimeSpan.FromSeconds(10),
                BreakTime = TimeSpan.FromSeconds(20),
                Direction = TimePieceDirectionEnum.Back,
                EndTime = new TimeSpan(18, 15, 0),
                Mode = TimePieceModeEnum.Timer,
                ReferencePoint = TimePieceReferencePointEnum.ToSpecificTime,
                UseBreak = true,
                TextAbove = "Czasu do oznaczenia przewodniczacego:",
                TextBelow = $"{PersonsRoles.Przewodniczacy}: {chairman}"
            };
            var xmlSerializer = new CustomXmlSerializer(timePieceVM.GetType());
            var timePieceRepository = MediaLocalFileRepositoryFactory.TimePiece;
            string timePieceFileName = $"{meeting.MeetingTitle}.tim";
            using (Stream memStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memStream, timePieceVM);
                timePieceRepository.SaveFile(memStream, timePieceFileName);
            }
            firstPoint.Sources.Insert(0, BaseMediaFileInfo.Factory.Create(firstPoint, timePieceFileName, string.Empty));

            // Dodaj pieśni
            var christianWayOfLife = meeting.MeetingPointList.FirstOrDefault(x => x.PointTitle == "CHRZEŚCIJAŃSKI TRYB ŻYCIA");
            if (christianWayOfLife != null)
            {
                AddSong(christianWayOfLife, chairman, songs[1]);
                int christianWayOfLifeIndex = meeting.MeetingPointList.IndexOf(christianWayOfLife);
                var nextPoint = meeting.MeetingPointList[christianWayOfLifeIndex + 1];
                if (nextPoint.PointTitle.StartsWith(PointTitles.Piesn))
                    meeting.MeetingPointList.Remove(nextPoint);
            }

            var lastPoint = meeting.MeetingPointList.Last();
            AddSong(lastPoint, chairman, songs.Last());

            // Dodaj osoby
            AddPersons(meeting, persons);

            // Dodaj obraz nagłówka
            meeting.HeaderImage = meeting.MeetingPointList
                .Select(x => x.Sources.FirstOrDefault(s => s is ImageFileInfo))
                .Where(x => x != null)
                .FirstOrDefault()
                ?.FileName;                

            // Za 5 min. posprzątaj wszystkie przeglądarki
            Task.Factory.StartNew(() => 
            {
                Task.Delay(TimeSpan.FromMinutes(5));
                foreach (var form in _formList.ToList())
                {
                    Action closingAction = () => form.Close();
                    if (form.InvokeRequired)
                        form.Invoke(closingAction);
                    else
                        closingAction();
                }
            });

            return meeting;
        }

        private bool PointHavePerson(MeetingPointVM meetingPoint)
            => meetingPoint.PointTitle != meetingPoint.PointTitle.ToUpper() &&
              !meetingPoint.PointTitle.StartsWith(PointTitles.Piesn);

        private void AddPersons(MeetingVM meeting, List<string> persons)
        {
            int personIndex = 0;
            foreach (var p in meeting.MeetingPointList.Where(x => PointHavePerson(x)))
            {
                if (persons.Count == 0)
                    break;

                var parameters = p.ParameterList.Parameters;
                typeof(PersonsRoles).GetFields()
                    .Select(x => (string)x.GetValue(null))
                    .Select(x => parameters.Remove(parameters.FirstOrDefault(y => y.Key == x)))
                    .ToArray();

                if (p.PointTitle == PointTitles.CzytanieBiblii)
                    AddParameters(p.ParameterList, new Dictionary<string, string> { { PersonsRoles.Lektor, persons[personIndex] } });
                else if (p.PointTitle == PointTitles.PierwszaRozmowa ||
                         p.PointTitle == PointTitles.Odwiedziny ||
                         p.PointTitle == PointTitles.StudiumBiblijne)
                {
                    var sss = persons[personIndex]
                        .Split('+')
                        .Select(personFullName => personFullName.Trim().Split(' '))
                        .Select(personNameItem => new
                        {
                            IsWoomen = personNameItem.First()?.Last() == 'a',
                            Person = personNameItem.First() + " " + personNameItem.Last()
                        })
                        .ToList();

                    if (sss.Count() == 2)
                    {
                        AddParameters(p.ParameterList, new Dictionary<string, string>
                        {
                            {  sss.First().IsWoomen ? PersonsRoles.Prowadzaca : PersonsRoles.Prowadzacy, sss.First().Person },
                            {  sss.Last().IsWoomen  ? PersonsRoles.Pomocnica  : PersonsRoles.Pomocnik,   sss.Last().Person },
                        });
                    }
                }
                else if (p.PointTitle.Contains(PointTitles.ZboroweStudiumBiblii))
                {
                    var sss = persons[personIndex]
                        .Split('+')
                        .Select(personFullName => personFullName.Trim().Split(' '))
                        .Select(personNameItem => personNameItem.First() + " " + personNameItem.Last())
                        .ToList();
                    
                    if (sss.Count == 2)
                    {
                        AddParameters(p.ParameterList, new Dictionary<string, string>
                        {
                            { PersonsRoles.Prowadzacy, sss.First() },
                            { PersonsRoles.Lektor,     sss.Last()  }
                        });
                    }
                }
                else
                {
                    AddParameters(p.ParameterList, new Dictionary<string, string>
                    {
                        { PersonsRoles.Prowadzacy, persons[personIndex] }
                    });
                }
                personIndex++;
                if (personIndex >= persons.Count)
                    break;
            }
        }

        private class PersonsRoles
        {
            public const string Przewodniczacy = "Przewodniczący";
            public const string Prowadzacy = "Prowadzący";
            public const string Prowadzaca = "Prowadząca";
            public const string Lektor = "Lektor";
            public const string Pomocnik = "Pomocnik";
            public const string Pomocnica = "Pomocnica";
            public const string Host = "Host";
            public static string Porzadkowy = "Porządkowy";
        }

        private class PointTitles
        {
            public const string UwagiKoncowe = "Uwagi końcowe";
            public const string CzytanieBiblii = "Czytanie Biblii";
            public const string PierwszaRozmowa = "Pierwsza rozmowa";
            public const string Odwiedziny = "Odwiedziny";
            public const string StudiumBiblijne = "Studium biblijne";
            public const string Przemowienie = "Przemówienie";
            public const string WyszukujemyDuchoweSkarby = "Wyszukujemy duchowe skarby";
            public const string Piesn = "Pieśń";
            public const string ZboroweStudiumBiblii = "Zborowe studium Biblii";
        }

        private void AddSong(MeetingPointVM meetingPoint, string chairman, FileInfoFromWeb song) 
        {
            AddParameters(meetingPoint.ParameterList, new Dictionary<string, string>
            {
                { PersonsRoles.Przewodniczacy, chairman   },
                { PointTitles.Piesn,           song.Title }
            });
            meetingPoint.IsExpanded = true;
            AddSources(meetingPoint, new List<FileInfoFromWeb> { song });
        }

        private void AddParameters(ParametersCollectionVM paramsCollection, Dictionary<string, string> paramsDictionary)
        {
            if (paramsCollection.Parameters == null)
                paramsCollection.Parameters = new ObservableCollection<ParameterVM>();
            paramsDictionary
                .Select(p => new ParameterVM
                {
                    ParametersCollection = paramsCollection,
                    Key = p.Key,
                    Value = p.Value
                })
                .ToList()
                .ForEach(p => paramsCollection.Parameters.Add(p));
        }

        private void AddMeetingPoint(HtmlNode pointHtmlNode, Color titleColor, bool scanForMultimedia, MeetingVM meeting, Dictionary<string, string> parameters) 
        {
            string pointTitle = GetPointTitle(pointHtmlNode);
            if (pointTitle == PointTitles.UwagiKoncowe)
                return;

            string[] pointHyperLinks = pointHtmlNode
                .GetHtmlNodes(n => n.Name == "a")
                .Select(x => x.Attributes["href"].Value)
                .Select(x => x.StartsWith(BaseAddress) ? x : BaseAddress + x)
                .ToArray();

            string webAddress = pointHyperLinks.Any()
                ? pointHyperLinks.First()
                : " ";

            var meetingPoint = new MeetingPointVM
            {
                PointTitle = pointTitle,
                WebAddress = webAddress,
                Duration = GetDuration(pointHtmlNode),
                IsExpanded = true,
                Meeting = meeting,
                TitleColor = titleColor, 
                Indent = 1
            };
            if(parameters != null)
                AddParameters(meetingPoint.ParameterList, parameters);

            if (scanForMultimedia && DownloadMultimedia(pointHtmlNode))
            {
                foreach (var href in pointHyperLinks)
                {
                    var pngFiles = GetJpgFileInfoListFromArticle(href);
                    var mp4Files = GetMp4FileInfoListFromArticle(pointTitle, href);
                    var files = pngFiles.Union(mp4Files).OrderBy(f => f.Lp).ToList();
                    AddSources(meetingPoint, files);
                }
            }
            meeting.MeetingPointList.Add(meetingPoint);
        }

        public List<MeetingPointVM> GetPrimitiveMeetingPoints(DateTime dateTime)
        {
            var meeting = new MeetingVM();
            meeting.WebAddress = GetAddress(dateTime);

            string meetingPointListHtml;
            using (var webClient = new WebClient())
            {
                meetingPointListHtml = webClient.DownloadString(new Uri(meeting.WebAddress));
            }
            var meetingPointListHtmlDoc = new HtmlDocument();
            meetingPointListHtmlDoc.LoadHtml(meetingPointListHtml);

            HtmlNode articleHtmlNode = meetingPointListHtmlDoc.DocumentNode.GetHtmlNode((n) => n.Name == "article");           
            HtmlNode bodyTxtNode = articleHtmlNode.GetHtmlNode(n => n.Name == "div" && n.HasClass("bodyTxt"));
            CreateMeetingPoints(meeting, bodyTxtNode, false);
            foreach (var p in meeting.MeetingPointList)
                p.IsExpanded = false;

            meeting.MeetingPointList = new ObservableCollection<MeetingPointVM>(
                meeting.MeetingPointList
                .Where(m => PointHavePerson(m)));

            meeting.MeetingPointList.FirstOrDefault()?.Sorter.Sort();

            return meeting.MeetingPointList.ToList(); ;
        }

        public void CreateMeetingPoints(
            MeetingVM meeting,
            HtmlNode bodyTxtNode,
            bool scanForMultimedia)
        {
            var sectionConfigList = new List<(string HtmlNodeId, Color TitleColor)>
            {
                ("section2", Colors.Gray),
                ("section3", Colors.Peru),
                ("section4", Colors.IndianRed)
            };
            foreach (var sectionConfig in sectionConfigList)
            {
                HtmlNode section = bodyTxtNode.GetHtmlNode(n => n.Id == sectionConfig.HtmlNodeId);

                // Nagłówek (część spotkania)
                var headerMeetingPoint = new MeetingPointVM
                {
                    PointTitle = GetPointHeaderTitle(section),
                    IsExpanded = false,
                    Meeting = meeting,
                    TitleColor = sectionConfig.TitleColor
                };
                meeting.MeetingPointList.Add(headerMeetingPoint);

                // Lista punktów
                List<HtmlNode> pointListHtmlNodes = section
                    .GetHtmlNode(n => n.Name == "ul")
                    .Elements("li").ToList();

                // Dodaj punkty
                pointListHtmlNodes.ForEach(x => AddMeetingPoint(x, sectionConfig.TitleColor, scanForMultimedia, meeting, null));
            }
        }

        private string GetPointTitle(HtmlNode node)
            => node.GetHtmlNodes(n => n.Name == "strong")
                    .Select(x => x.InnerText)
                    .Aggregate((a, b) => a + " " + b)
                    .Trim();

        private string GetPointHeaderTitle(HtmlNode node)
            => node.GetHtmlNode(n => n.Name == "h2").InnerText;

        private bool DownloadMultimedia(HtmlNode node)
        {
            string pointTitle = GetPointTitle(node);
            return pointTitle != PointTitles.PierwszaRozmowa &&
                   pointTitle != PointTitles.Odwiedziny &&
                   pointTitle != PointTitles.StudiumBiblijne &&
                   pointTitle != PointTitles.Przemowienie &&
                   pointTitle != PointTitles.CzytanieBiblii &&
                   pointTitle != PointTitles.WyszukujemyDuchoweSkarby &&
                   !pointTitle.Contains(PointTitles.Piesn);
        }

        private TimeSpan GetDuration(HtmlNode hyperLinkHtmlNode)
        {
            string nodeInnerText = hyperLinkHtmlNode.InnerText;
            if (string.IsNullOrEmpty(nodeInnerText))
                return TimeSpan.Zero;
            List<string> positionsList = nodeInnerText.Split('(').ToList();
            if (positionsList.Count > 1)
                positionsList.RemoveAt(0);
            else
                return TimeSpan.Zero;

            foreach (string position in positionsList)
            {
                int lastCharNr = position.IndexOf(')');
                if (lastCharNr == -1)
                    continue;
                string value = position.Substring(0, lastCharNr)
                    .Replace("(", null)
                    .Replace(")", null)
                    .Replace(".", null)
                    .Replace("min", null)
                    .Trim();
                if (int.TryParse(value, out int minutes))
                {
                    return TimeSpan.FromMinutes(minutes);
                }
            }
            return TimeSpan.Zero;
        }

        private void AddSources(MeetingPointVM xpoint1, List<FileInfoFromWeb> files) 
        {
            foreach (FileInfoFromWeb f in files)
            {
                if (f.IsCompletted)
                {
                    string fileName = f.FileSource.Contains("https://www.jw.org/finder?") 
                        ? f.FileSource
                              .Replace("https://www.jw.org/finder?lank=pub-", null)
                              .Replace("VIDEO&amp;wtlocale=P", "r720P.mp4")    
                        : f.FileSource.Split('/').Last();
                    var newSource = BaseMediaFileInfo.Factory.Create(xpoint1, fileName, f.FileSource);
                    newSource.Title = f.Title;
                    newSource.WebAddress = f.FileSource;
                    xpoint1.Sources.Add(newSource);
                }
                else
                {
                    f.OnCompletted += (s, e) => 
                    {
                        var newFileInfoFromWeb = (FileInfoFromWeb)s;
                        string shortFileName = PathHelper.GetShortFileName(newFileInfoFromWeb.FileSource, '/');
                        var meetingPoint = SingletonVMFactory.Meeting.MeetingPointList.FirstOrDefault(x => x.PointTitle == xpoint1.PointTitle);
                        if (meetingPoint?.Sources?.Any(x => x.FileName == shortFileName) == false)
                        {
                            var newMediaFileInfo = BaseMediaFileInfo.Factory.Create(meetingPoint, shortFileName, newFileInfoFromWeb.FileSource);
                            newMediaFileInfo.CheckFileExist();
                            newMediaFileInfo.FillMetadata();
                            meetingPoint.Sources.Add(newMediaFileInfo);
                            newMediaFileInfo.Sorter.Sort();
                        }
                    };
                }
            }
            xpoint1.Source = xpoint1.Sources.FirstOrDefault();
        }
    }
}
