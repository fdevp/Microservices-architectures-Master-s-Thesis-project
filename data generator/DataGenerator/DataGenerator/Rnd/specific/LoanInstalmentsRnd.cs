using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator.Rnd
{
    public class LoanInstalmentsRnd : Rnd<int>
    {
        private int[] shortLoan = new[] { 6, 12, 18, 24 };
        private int[] midLoan = new[] { 36, 48, 60, 96 };
        private int[] longLoan = new[] { 120, 180, 240, 360 };

        public override int Next()
        {
            throw new NotImplementedException();
        }

        public int Next(float totalAmount)
        {
            if (totalAmount < 25000)
                return shortLoan[rand.Next(0, shortLoan.Length)];
            else if (totalAmount < 100000)
                return midLoan[rand.Next(0, shortLoan.Length)];
            else
                return longLoan[rand.Next(0, shortLoan.Length)];
        }
    }
}
