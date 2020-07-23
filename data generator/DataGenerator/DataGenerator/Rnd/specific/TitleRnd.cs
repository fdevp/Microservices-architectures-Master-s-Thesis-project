using System;
using System.Collections.Generic;
using System.Text;
using LoremNET;

namespace DataGenerator.Rnd
{
    public class TitleRnd : Rnd<string>
    {
        const int maxChars = 140;

        public override string Next()
        {
            var chars = rand.Next(0, maxChars);
            var sb = new StringBuilder();
            do
            {
                sb.Append(Lorem.Words(1));
                sb.Append(" ");
            } while (sb.Length < chars);

            return sb.ToString();
        }
    }
}
