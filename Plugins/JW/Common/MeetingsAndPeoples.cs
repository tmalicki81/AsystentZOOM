using System;
using System.Collections.Generic;

namespace AsystentZOOM.Plugins.JW.Common
{
    [Serializable]
    public class MeetingsAndPeoples
    {
        [Serializable]
        public class Meeting
        {
            public DateTime MeetingDay { get; set; }
            public string Host { get; set; }
            public string CoHost { get; set; }
            public string Chairman { get; set; }
            public List<string> PeopleList { get; set; } = new List<string>();
        }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public List<Meeting> MeetingList { get; set; } = new List<Meeting>();
    }
}
