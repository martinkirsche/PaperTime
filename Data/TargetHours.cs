using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaperTime.Data
{
    public class TargetHours
    {
        public DateTime ValidUntil { get; set; }
        public TimeSpan Value { get; set; }
    }
}
