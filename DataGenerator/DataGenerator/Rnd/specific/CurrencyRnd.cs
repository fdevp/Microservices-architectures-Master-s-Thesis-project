using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator.Rnd
{
    public class CurrencyRnd : Rnd<float>
    {
        private readonly bool cents;

        public CurrencyRnd(bool cents = true)
        {
            this.cents = cents;
        }

        public override float Next()
        {
            var cent = cents ? (float)Math.Round((float)rand.NextDouble(), 2) : 0;
            if (DistributionFormula != null && DistributionValuesProbabilities != null)
            {
                return TakeDrawn(DistributionFormula.Invoke()) + cent;
            }

            if (DistributionFormula != null && MinSet && MaxSet)
            {
                return DistributionFormula.Invoke() * Max + Min;
            }

            if (DistributionValues != null && MinSet && MaxSet)
            {
                var drawn = TakeDrawn(rand.Next(ProbabilitiesSum), out var index);
                var min = index == 0 ? Min : DistributionValues[index - 1];
                var max = index == DistributionValues.Length - 1 ? Max : DistributionValues[index + 1];
                return rand.Next((int)min, (int)max) + cent;
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
                return rand.Next((int)Min, (int)Max) + cent;
            }

            throw new InvalidOperationException("Unknown calculations");
        }
    }
}
