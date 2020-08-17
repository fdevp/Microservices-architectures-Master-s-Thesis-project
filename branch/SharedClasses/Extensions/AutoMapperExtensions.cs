using System;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace SharedClasses
{
    public static class AutoMapperExtensions
    {
        public static void AddGrpcConverters(this IMapperConfigurationExpression expression)
        {
            expression.CreateMap<DateTime, Timestamp>().ConvertUsing(new DateTimeTypeConverter());
            expression.CreateMap<Timestamp, DateTime>().ConvertUsing(new DateTimeTypeConverterReverse());
            expression.CreateMap<TimeSpan, Duration>().ConvertUsing(new TimeSpanTypeConverter());
            expression.CreateMap<Duration, TimeSpan>().ConvertUsing(new TimeSpanTypeConverterReverse());
        }
    }
}