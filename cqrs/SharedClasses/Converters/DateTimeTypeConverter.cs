using System;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using SharedClasses;

namespace SharedClasses
{
    public class DateTimeTypeConverter : ITypeConverter<DateTime, Timestamp>
    {
        public Timestamp Convert(DateTime source, Timestamp destination, ResolutionContext context)
        {
            return source.ToNullableTimestamp();
        }
    }

    public class DateTimeTypeConverterReverse : ITypeConverter<Timestamp, DateTime>
    {
        public DateTime Convert(Timestamp source, DateTime destination, ResolutionContext context)
        {
            return source.ToDateTime();
        }
    }
}