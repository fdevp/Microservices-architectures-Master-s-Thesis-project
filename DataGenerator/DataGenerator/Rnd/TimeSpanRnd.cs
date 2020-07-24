using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator.Rnd
{
    public class TimeSpanRnd : Rnd<TimeSpan>
    {
        public override TimeSpan Next()
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
                return TimeSpan.FromMinutes(rand.Next((int)Min.TotalMinutes, (int)Max.TotalMinutes));
            }

            throw new InvalidOperationException("Unknown calculations");
        }
    }
}
