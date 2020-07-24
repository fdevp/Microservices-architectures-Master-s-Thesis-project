using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator
{
    public interface IRnd<T>
    {
        T Next();
        T Next(params T[] except);
    }
}
