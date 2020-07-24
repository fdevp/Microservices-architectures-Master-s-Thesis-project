using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator.Rnd
{
    class DateTimeRnd : Rnd<DateTime>
    {
        public override DateTime Next()
        {
            if (DistributionFormula != null && DistributionValuesProbabilities != null)
            {
                return TakeDrawn(DistributionFormula.Invoke());
            }

            if (DistributionFormula != null && MinSet && MaxSet)
            {
                var rnd = (int)(Max - Min).TotalMinutes;
                return Min + TimeSpan.FromMinutes(DistributionFormula.Invoke() * rnd);
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
                return Min + TimeSpan.FromMinutes(rand.Next(0, (int)(Max - Min).TotalMinutes));
            }

            throw new InvalidOperationException("Unknown calculations");
        }
    }
}
