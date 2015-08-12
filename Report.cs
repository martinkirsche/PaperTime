using System;
using System.Collections.Generic;
using PaperTime.Data;

namespace PaperTime
{
    public class Report
    {
        static Report empty =
            new Report()
            {
                FileName = string.Empty,
                FirstDay = DateTime.MinValue,
                LastDay = DateTime.MaxValue,
                Categories = new Dictionary<string, TimeSpan>(),
                CategoriesOverall = new Dictionary<string, TimeSpan>(),
                Holidays = new List<Holiday>(),
                TargetHours = new List<TargetHours>(),
                Records = new List<Record>(),
                Days = 0,
                TimeOverall = TimeSpan.Zero,
                TimeActual = TimeSpan.Zero,
                TimePreviousSaldo = TimeSpan.Zero,
                TimeTarget = TimeSpan.Zero
            };

        public static Report Empty { get { return empty; } }

        public string FileName { get; set; }
        public DateTime FirstDay { get; set; }
        public DateTime LastDay { get; set; }
        public List<Holiday> Holidays { get; set; }
        public List<TargetHours> TargetHours { get; set; }
        public List<Record> Records { get; set; }
        public int Days { get; set; }
        public TimeSpan TimeOverall { get; set; }
        public TimeSpan TimeTarget { get; set; }
        public TimeSpan TimeActual { get; set; }
        public TimeSpan TimePreviousSaldo { get; set; }
        public TimeSpan TimeSaldo { get { return TimePreviousSaldo + (TimeActual - TimeTarget); } }
        public IDictionary<string, TimeSpan> Categories { get; set; }
        public IDictionary<string, TimeSpan> CategoriesOverall { get; set; }

        public static bool CategoryIsRoot(string categoryPath)
        {
            return categoryPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length == 1;
        }
    }
}
