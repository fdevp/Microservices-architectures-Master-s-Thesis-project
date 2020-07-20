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
            var euros = (float)rand.Next();
            if (cents)
                euros += (float)rand.NextDouble();

            return euros;
        }
    }
}
