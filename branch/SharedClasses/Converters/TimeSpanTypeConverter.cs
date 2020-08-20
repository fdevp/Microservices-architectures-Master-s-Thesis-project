using System;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace SharedClasses
{
    public class TimeSpanTypeConverter : ITypeConverter<TimeSpan, Duration>
    {
        public Duration Convert(TimeSpan source, Duration destination, ResolutionContext context)
        {
            return Duration.FromTimeSpan(source);
        }
    }

    public class TimeSpanTypeConverterReverse : ITypeConverter<Duration, TimeSpan>
    {
        public TimeSpan Convert(Duration source, TimeSpan destination, ResolutionContext context)
        {
            return source.ToTimeSpan();
        }
    }
}