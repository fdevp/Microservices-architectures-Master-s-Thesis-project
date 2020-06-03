using System;
using AutoMapper;

namespace APIGateway
{
    public class TimeSpanTypeConverter : ITypeConverter<TimeSpan, long>
    {
        public long Convert(TimeSpan source, long destination, ResolutionContext context)
        {
            return source.Ticks;
        }
    }

    public class TimeSpanTypeConverterReverse : ITypeConverter<long, TimeSpan>
    {
        public TimeSpan Convert(long source, TimeSpan destination, ResolutionContext context)
        {
            return TimeSpan.FromTicks(source);
        }
    }
}