using System;
using System.Collections.Generic;
using System.Text;

namespace Requester.Data
{
    public class AutomatSettings
    {
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        public TimeSpan TimeDelta { get; set; }
        public DateTime CurrentDate { get; set; }
        public DateTime DateLimit { get; set; }
    }
}
