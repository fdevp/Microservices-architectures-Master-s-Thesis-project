using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator.Rnd
{
    public class EnumRnd<T> : Rnd<T>
    {
        public override T Next()
        {
            Type type = typeof(T);
            Array values = type.GetEnumValues();
            int index = rand.Next(values.Length);
            return (T)values.GetValue(index);
        }
    }
}
