using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator
{
    public class IndividualUserScenarioElement
    {
        public string User { get; set; }
        public string AccountId { get; set; }
        public int Group { get; set; }
        public float Amount { get; set; }
        public string CardId { get; set; }
        public string Recipient { get; set; }
    }
}
