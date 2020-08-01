using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator
{
    public class BusinessUserScenarioElement
    {
        public string User { get; set; }
        public string UserId { get; set; }
        public int Group { get; set; }
        public BusinessUserTransaction[] Transactions { get; set; }
    }

    public class BusinessUserTransaction
    {
        public float Amount { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Title { get; set; }
    }

}
