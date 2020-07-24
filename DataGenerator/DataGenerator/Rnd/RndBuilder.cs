using DataGenerator.Rnd;
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
            this.rnd = rnd ?? CreateRnd();
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

        private Rnd<T> CreateRnd()
        {
            var typeName = typeof(T).Name;
            switch (typeName)
            {
                case "Int32":
                    return new IntRnd() as Rnd<T>;
                case "DateTime":
                    return new DateTimeRnd() as Rnd<T>;
                case "String":
                    return new StringRnd() as Rnd<T>;
                case "TimeSpan":
                    return new TimeSpanRnd() as Rnd<T>;
                default:
                    throw new InvalidOperationException("Unknown type.");
            }
        }
    }
}
