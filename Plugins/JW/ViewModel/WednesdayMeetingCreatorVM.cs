using AsystentZOOM.Plugins.JW.Common;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.ViewModel;
using JW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsystentZOOM.Plugins.JW.ViewModel
{
    public class WednesdayMeetingCreatorVM : BaseVM
    {
        private static DateTime? _lastMeetingDate;

        public WednesdayMeetingCreatorVM()
        {
            MeetingDate = _lastMeetingDate.HasValue
                ? _lastMeetingDate.Value
                : WeekHelper.GetDayOfWeek(DateTime.Now, MeetingDayOfWeek);
        }

        private MeetingsAndPeoples _meetingsAndPeoples;
        private MeetingsAndPeoples MeetingsAndPeoples
            => _meetingsAndPeoples ??= IsolatedStorageHelper.LoadObject<MeetingsAndPeoples>();

        private void SaveMeetingsAndPeoples() 
        {
            if (MeetingPointList?.Any() != true)
                return;
            
            MeetingsAndPeoples.LastUpdated = DateTime.Now;
            var selectedMonday = WeekHelper.GetDayOfWeek(MeetingDate, MeetingDayOfWeek);
            var mondayFromCache = MeetingsAndPeoples.MeetingList.FirstOrDefault(m => m.MeetingDay == selectedMonday);
            if (mondayFromCache == null)
            {
                mondayFromCache = new MeetingsAndPeoples.Meeting
                {
                    MeetingDay = selectedMonday
                };
                MeetingsAndPeoples.MeetingList.Add(mondayFromCache);
            }
            mondayFromCache.Host = Host;
            mondayFromCache.CoHost = CoHost;
            mondayFromCache.Chairman = Chairman;
            mondayFromCache.PeopleList = MeetingPointList
                .Select(x => x.Value)
                .ToList();
            IsolatedStorageHelper.SaveObject(MeetingsAndPeoples);
        }

        public DayOfWeek MeetingDayOfWeek = DayOfWeek.Wednesday;

        private DateTime _meetingDate;
        public DateTime MeetingDate
        {
            get => _meetingDate;
            set
            {
                if (_meetingDate == value)
                    return;

                SaveMeetingsAndPeoples();

                SetValue(ref _meetingDate, value, nameof(MeetingDate));
                _lastMeetingDate = value;

                var downloader = new WednesdayMeetingsDownloader();
                MeetingPointList = downloader
                    .GetPrimitiveMeetingPoints(MeetingDate)
                    .Select(x => new KvpVM<MeetingPointVM, string>
                    {
                        Key = x,
                        Value = string.Empty
                    })
                    .ToList();
                
                var selectedMonday = WeekHelper.GetDayOfWeek(MeetingDate, MeetingDayOfWeek);
                var mondayFromCache = MeetingsAndPeoples.MeetingList.FirstOrDefault(m => m.MeetingDay == selectedMonday);
                if (mondayFromCache != null)
                {
                    Host = mondayFromCache.Host;
                    CoHost = mondayFromCache.CoHost;
                    Chairman = mondayFromCache.Chairman;
                    for (int index = 0; index < mondayFromCache.PeopleList.Count; index++)
                    {
                        if (MeetingPointList.Count < index)
                            break;
                        MeetingPointList[index].Value = mondayFromCache.PeopleList[index];
                    }
                }
                else
                {
                    Host = string.Empty;
                    CoHost = string.Empty;
                    Chairman = string.Empty;
                }
            }
        }

        private string _chairman;
        public string Chairman
        {
            get => _chairman;
            set => SetValue(ref _chairman, value, nameof(Chairman));
        }

        private string _host;
        public string Host
        {
            get => _host;
            set => SetValue(ref _host, value, nameof(Host));
        }

        private string _coHost;
        public string CoHost
        {
            get => _coHost;
            set => SetValue(ref _coHost, value, nameof(CoHost));
        }

        private List<KvpVM<MeetingPointVM, string>> _meetingPointList;
        public List<KvpVM<MeetingPointVM, string>> MeetingPointList
        {
            get => _meetingPointList;
            set => SetValue(ref _meetingPointList, value, nameof(MeetingPointList));
        }

        public List<string> PeoplesList
            => MeetingPointList.Select(x => x.Value).ToList();

        private RelayCommand _previousMeetingCommand;
        public RelayCommand PreviousMeetingCommand
            => _previousMeetingCommand ??= new RelayCommand(PreviousMeeting);

        private void PreviousMeeting()
            => MeetingDate = MeetingDate.AddDays(-7);

        private RelayCommand _NextMeetingCommand;
        public RelayCommand NextMeetingCommand
            => _NextMeetingCommand ??= new RelayCommand(NextMeeting);

        private void NextMeeting()
            => MeetingDate = MeetingDate.AddDays(7);

        private RelayCommand _openDbFileCommand;
        public RelayCommand OpenDbFileCommand
            => _openDbFileCommand ??= new RelayCommand(OpenDbFile);
        
        private void OpenDbFile() 
        {
            IsolatedStorageHelper.SaveObject(MeetingsAndPeoples);
            IsolatedStorageHelper.OpenObject<MeetingsAndPeoples>();
        }

        private RelayCommand _createMeetingCommand;
        public RelayCommand CreateMeetingCommand
            => _createMeetingCommand ??= new RelayCommand(CreateMeeting);

        private void CreateMeeting()
        {
            SaveMeetingsAndPeoples();
            DialogHelper.RunAsync("Zebranie środowe", true, "Inicjalizacja", CreateMeetingAsync);
        }

        private async void CreateMeetingAsync(ProgressInfoVM progressInfo)
        {
            await SingletonVMFactory.Meeting.SaveLocalFile(false);

            var downloader = new WednesdayMeetingsDownloader();
            MeetingVM meeting = downloader.CreateMeeting(
                MeetingDate,
                Chairman,
                Host, 
                CoHost,
                PeoplesList);

            MainVM.Dispatcher.Invoke(() => SingletonVMFactory.SetSingletonValues(meeting));

            meeting.DownloadAndFillMetadata(progressInfo);
            SingletonVMFactory.Meeting.ClearLocalFileName();
        }
    }
}
