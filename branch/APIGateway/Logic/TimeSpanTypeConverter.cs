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
}