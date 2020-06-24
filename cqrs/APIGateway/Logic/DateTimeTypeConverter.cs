using System;
using AutoMapper;

namespace APIGateway
{
    public class DateTimeTypeConverter : ITypeConverter<DateTime, long>
    {
        public long Convert(DateTime source, long destination, ResolutionContext context)
        {
            return source.Ticks;
        }
    }

    public class DateTimeTypeConverterReverse : ITypeConverter<long, DateTime>
    {
        public DateTime Convert(long source, DateTime destination, ResolutionContext context)
        {
            return new DateTime(source);
        }
    }
}