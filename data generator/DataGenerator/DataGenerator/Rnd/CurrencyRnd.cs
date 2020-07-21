﻿using System;
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
            var cent = cents ? (float)rand.NextDouble() : 0;
            if (DistributionFormula != null && DistributionValuesProbabilities != null)
            {
                return TakeDrawn(DistributionFormula.Invoke()) + cent;
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
                return rand.Next((int)Min, (int)Max) + cent;
            }

            throw new InvalidOperationException("Unknown calculations");
        }
    }
}
