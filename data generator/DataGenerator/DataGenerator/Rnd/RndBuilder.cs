using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator
{
    public class RndBuilder<T>
    {
        Rnd<T> rnd;

        public RndBuilder(Rnd<T> rnd = null)
        {
            rnd = rnd ?? new Rnd<T>();
        }

        public IRnd<T> Build()
        {
            return rnd;
        }

        public RndBuilder<T> DistributionValues(IEnumerable<T> values)
        {
            rnd.DistributionValues = values.ToArray();
            return this;
        }

        public RndBuilder<T> DistributionProbabilities(IEnumerable<int> probabilities)
        {
            rnd.DistributionValuesProbabilities = probabilities.ToArray();
            return this;
        }

        public RndBuilder<T> Max(T max)
        {
            rnd.Max = max;
            return this;
        }

        public RndBuilder<T> Min(T min)
        {
            rnd.Min = min;
            return this;
        }
    }

    public abstract class Rnd<T> : IRnd<T>
    {
        protected Random rand = new Random();

        public T Min { get; set; }
        public T Max { get; set; }

        public T[] DistributionValues { get; set; }

        public int[] DistributionValuesProbabilities { get; set; }

        public Func<int> DistributionFormula { get; set; }

        public abstract T Next();

        public virtual T Next(params T[] except)
        {
            T value;
            do
            {
                value = Next();
            } while (except.Contains(value));

            return value;
        }
    }

    
}
