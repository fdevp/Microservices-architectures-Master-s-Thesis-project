using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator.Rnd
{
    public abstract class Rnd<T> : IRnd<T>
    {
        protected Random rand = new Random();

        public T Min { get => min; set { min = value; MinSet = true; } }

        private T min;
        protected bool MinSet;

        public T Max { get => max; set { max = value; MaxSet = true; } }

        private T max;
        protected bool MaxSet;

        public Func<int> DistributionFormula { get; set; }

        public T[] DistributionValues { get; set; }

        public int[] DistributionValuesProbabilities
        {
            get => probabilities;
            set
            {
                probabilities = value;
                ProbabilitiesSum = probabilities.Sum(p => p);
            }
        }

        private int[] probabilities;

        public int ProbabilitiesSum { get; private set; }
        
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

        protected T TakeDrawn(int drawn)
        {
            return TakeDrawn(drawn, out var index);
        }

        protected T TakeDrawn(int drawn, out int index)
        {
            int sum = 0;
            for (int i = 0; i < DistributionValuesProbabilities.Length; i++)
            {
                sum += DistributionValuesProbabilities[i];
                if (sum > drawn)
                {
                    index = i;
                    return DistributionValues[i];
                }
            }

            throw new InvalidOperationException("Invalid probabilities.");
        }
    }
}
