using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator.Rnd
{
    public class IntRnd : Rnd<int>
    {
        public override int Next()
        {
            if (DistributionFormula != null && DistributionValuesProbabilities != null)
            {
                return TakeDrawn(DistributionFormula.Invoke());
            }

            if (DistributionFormula != null && MinSet && MaxSet)
            {
                return DistributionFormula.Invoke() * Max + Min;
            }

            if (DistributionValuesProbabilities != null)
            {
                return TakeDrawn(rand.Next(ProbabilitiesSum));
            }

            if (DistributionValues != null)
            {
                return DistributionValues[rand.Next(DistributionValues.Length)];
            }

            if (MinSet && MaxSet)
            {
                return rand.Next(Min, Max + 1);
            }

            throw new InvalidOperationException("Unknown calculations");
        }
    }
}
