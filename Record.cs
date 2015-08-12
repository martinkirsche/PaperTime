using System;
using System.Collections.Generic;

namespace PaperTime
{
    public class Record
    {
        public Record()
        {
            Message = new List<string>();
        }

        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public IList<string> Message { get; set; }
        public string Category { get; set; }
        public double Multiplier { get; set; }
        public TimeSpan Duration { get; set; }

        public Record Clone()
        {
            return new Record()
            {
                Begin = Begin,
                End = End,
                Message = new List<string>(Message),
                Category = Category,
                Multiplier = Multiplier,
                Duration = Duration
            };
        }
    }
}
