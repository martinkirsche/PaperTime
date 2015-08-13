using PaperTime.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaperTime
{
    public class Reporter
    {
        List<TargetHours> targetHours = new List<TargetHours>();

        public void AddTargetHours(DateTime validUntil, TimeSpan value)
        {
            targetHours.Add(new TargetHours() { ValidUntil = validUntil, Value = value });
        }

        Dictionary<DateTime, Holiday> knownHolidays = new Dictionary<DateTime, Holiday>();

        public bool AddKnownHolidays(string name, DateTime date)
        {
            date = date.Date;
            if (knownHolidays.ContainsKey(date)) { return false; }
            knownHolidays.Add(date, new Holiday() { Date = date, Name = name });
            return true;
        }

        List<DayOfWeek> workdays = new List<DayOfWeek>();

        public void AddWorkday(DayOfWeek day)
        {
            workdays.Add(day);  
        }

        public string FileNamePattern { get; set; }

        public string GetFileName(DateTime firstDate, DateTime lastDate)
        {
            return string.Format(FileNamePattern, lastDate);
        }

        public IEnumerable<DateTime> GroupBoundaries(DateTime firstDay, DateTime lastDay)
        {
            lastDay = lastDay.Date;
            firstDay = firstDay.Date;
            DateTime currentDay = firstDay;
            while (true)
            {
                string lastFileName = GetFileName(currentDay, currentDay);
                do
                {
                    currentDay += TimeSpan.FromDays(1);
                    if (currentDay > lastDay) { yield break; }
                } while (GetFileName(currentDay, currentDay) == lastFileName);               
                yield return currentDay.Date;
            }
        }

        public IEnumerable<Report> Generate(IEnumerable<Record> records)
        {
            records = records.OrderBy(r => r.Begin);
            DateTime firstDay = records.First().Begin.Date;
            DateTime LastDay = records.Last().Begin.Date;
            DateTime lastBoundry = firstDay;
            var previousReport = new Report();
            foreach (DateTime boundary in GroupBoundaries(firstDay, LastDay))
            {
                var b = boundary - TimeSpan.FromTicks(1);
                var groupRecords = records.Where(r => r.Begin >= lastBoundry && r.Begin <= b).ToList();
                previousReport = Generate(previousReport, groupRecords,
                    lastBoundry,
                    b);
                lastBoundry = boundary;
                yield return previousReport;
            }
            {
                var groupRecords = records.Where(r => r.Begin >= lastBoundry).ToList();
                if (groupRecords.Count == 0) { yield break; }
                yield return Generate(previousReport, groupRecords,
                    lastBoundry,
                    records.Last().Begin.Date);
            }
        }

        private Report Generate(Report previousReport, IEnumerable<Record> records, DateTime firstDay, DateTime lastDay)
        {
            List<Holiday> holidays;
            List<TargetHours> utilizedTargetHours;
            int days;
            TimeSpan timeTarget;
            FindWorkdays(firstDay, lastDay, out utilizedTargetHours, out holidays, out days, out timeTarget);
            TimeSpan timeActual;
            var categories = CalculateCategories(records, out timeActual);
            var categoriesOverall = CategoriesSum(
                previousReport.CategoriesOverall ?? new Dictionary<string, TimeSpan>(), categories);
            var timeOverall = categoriesOverall
                .Where(kvp => Report.CategoryIsRoot(kvp.Key))
                .Select(kvp => kvp.Value)
                .Aggregate((s, t) => s + t);
            return
                new Report()
                {
                    FileName = GetFileName(firstDay, lastDay),
                    FirstDay = firstDay,
                    LastDay = lastDay,
                    Records = records.ToList(),
                    Holidays = holidays,
                    TargetHours = utilizedTargetHours,
                    Days = days,
                    TimeOverall = timeOverall,
                    TimeActual = timeActual,
                    TimeTarget = timeTarget,
                    TimePreviousSaldo = previousReport.TimeSaldo,
                    Categories = categories,
                    CategoriesOverall = categoriesOverall
                };
        }

        private IDictionary<string, TimeSpan> CategoriesSum(IDictionary<string, TimeSpan> a, IDictionary<string, TimeSpan> b)
        {
            IDictionary<string, TimeSpan> categories = new SortedDictionary<string, TimeSpan>();
            foreach (var key in a.Keys.Union(b.Keys).Distinct())
            {
                var time = TimeSpan.Zero;
                if (a.ContainsKey(key))
                {
                    time += a[key];
                } 
                if (b.ContainsKey(key))
                {
                    time += b[key];
                }
                categories.Add(key, time);
            }
            return categories;
        }

        void FindWorkdays(DateTime from, DateTime to, out List<TargetHours> utilizedTargetHours, out List<Holiday> holidays, out int days, out TimeSpan timeTarget)
        {
            DateTime d = from;
            var targetHoursQueue = new Queue<TargetHours>(targetHours.OrderBy(x => x.ValidUntil));
            timeTarget = TimeSpan.Zero;
            days = 0;
            holidays = new List<Holiday>();
            utilizedTargetHours = new List<TargetHours>();

            TargetHours currentTargetHours = null;
            while (d <= to)
            {
                if (workdays.Contains(d.DayOfWeek))
                {
                    if (knownHolidays.ContainsKey(d))
                    {
                        holidays.Add(knownHolidays[d]);
                    }
                    else
                    {
                        if (null == currentTargetHours ||
                            currentTargetHours.ValidUntil.Date < d.Date)
                        {
                            do
                            {
                                currentTargetHours = targetHoursQueue.Dequeue();
                            } while (currentTargetHours.ValidUntil.Date < d.Date);
                            utilizedTargetHours.Add(currentTargetHours);
                        }
                        timeTarget += currentTargetHours.Value;
                        days++;
                    }
                }
                d = d.AddDays(1);
            }
        }

        IDictionary<string, TimeSpan> CalculateCategories(IEnumerable<Record> records, out TimeSpan totalTime)
        {
            totalTime = TimeSpan.Zero;

            IDictionary<string, TimeSpan> categories = new SortedDictionary<string, TimeSpan>();
            foreach (Record r in records)
            {
                string[] tags = r.Category.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                string path = string.Empty;
                foreach (string t in tags)
                {
                    path += "/" + t;
                    if (!categories.ContainsKey(path)) { categories[path] = TimeSpan.Zero; }
                    categories[path] += r.Duration;
                }
                totalTime += r.Duration;
            }
            return categories;
        }
    }
}
