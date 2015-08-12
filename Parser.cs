using System;
using System.Collections.Generic;
using System.IO;

namespace PaperTime
{
    public class Parser
    {
        public TimeSpan MaxDurationLimit { get; set; }

        public Parser()
        {
            MaxDurationLimit = TimeSpan.MaxValue;
        }

        public IEnumerable<Record> Load(IEnumerable<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                foreach (Record r in Load(fileName))
                {
                    yield return r;
                }
            }
        }

        public IEnumerable<Record> Load(string fileName)
        {
            IEnumerable<string> lines;
            using (StreamReader sr = new StreamReader(fileName))
            {
                lines = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            }
            return LoadLines(lines);
        }

        public IEnumerable<Record> LoadLines(IEnumerable<string> lines)
        {
            int counter = 0;
            Record current = null;
            foreach (string line in lines)
            {
                counter++;

                if (string.IsNullOrEmpty(line.Trim())) { continue; }

                if (current != null && line.Length >= 3 &&
                    line.Trim().Split(new char[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries).Length == 0)
                {
                    current = null;
                    continue;
                }

                string[] parts =
                    line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                DateTime date = DateTime.MinValue;

                if (parts.Length < 2 || !DateTime.TryParse(parts[0] + " " + parts[1], out date))
                {
                    if (current != null) { current.Message.Add(line); }
                    continue;
                }

                Record newRecord = new Record() { Begin = date };

                if (parts.Length >= 3)
                {
                    newRecord.Category = parts[2];
                    var m = multiplyer.Find(x => newRecord.Category.Contains(x.CategoryFilter));
                    newRecord.Multiplier = null == m ? 1.0 : m.Value;                    
                }

                if (current == null)
                {
                    current = newRecord;
                    continue;
                }
                if (string.IsNullOrEmpty(current.Category))
                {
                    throw new FormatException(counter, Text.ErrorCategoryEmpty);
                }
                current.End = date;
                if (current.Begin > current.End)
                {
                    throw new FormatException(counter, string.Format(Text.ErrorBeginPriorEnd, current.Begin, current.End));
                }
                current.Duration = new TimeSpan(Convert.ToInt64((current.End - current.Begin).Ticks * current.Multiplier));
                if (current.Duration > MaxDurationLimit)
                {
                    throw new FormatException(counter, string.Format(Text.ErrorMaxDurationLimit, MaxDurationLimit));
                }
                yield return current;
                current = newRecord;
                continue;
            }
        }

        class Multiplyer
        {
            public string CategoryFilter { get; set; }
            public double Value { get; set; }
        }

        List<Multiplyer> multiplyer = new List<Multiplyer>();

        public void AddMultiplyer(string categoryFilter, double value)
        {
            multiplyer.Add(new Multiplyer() { CategoryFilter = categoryFilter, Value = value });
        }
    }

    public class FormatException : Exception
    {
        public FormatException()
            : base()
        { }

        public FormatException(int line, string message)
            : base(string.Format(Text.ErrorInLine, line, message))
        { }
    }
}
