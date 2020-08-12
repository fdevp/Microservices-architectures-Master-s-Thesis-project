using System;
using System.Collections.Generic;
using System.Text;

namespace Requester.Data
{
    public class AutomatSettings
    {
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan SleepTime { get; set; }
    }
}
