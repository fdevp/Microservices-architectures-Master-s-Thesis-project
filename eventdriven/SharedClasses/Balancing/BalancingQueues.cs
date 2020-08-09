using System.Collections.Generic;
using System.Linq;

namespace SharedClasses.Balancing
{
    public class BalancingQueues
    {
        public string[] Queues { get; }
        
        public BalancingQueues(IEnumerable<string> queues)
        {
            this.Queues = queues.ToArray();
        }
    }
}