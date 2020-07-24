using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator.Rnd
{
    public class StringRnd : Rnd<string>
    {
        public override string Next()
        {
            if (DistributionValuesProbabilities != null)
            {
                return TakeDrawn(rand.Next(ProbabilitiesSum));
            }

            if (DistributionValues != null)
            {
                return DistributionValues[rand.Next(DistributionValues.Length)];
            }

            throw new InvalidOperationException("Unknown calculations");
        }
    }
}
