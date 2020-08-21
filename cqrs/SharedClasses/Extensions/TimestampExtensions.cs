using System;
using Google.Protobuf.WellKnownTypes;

namespace SharedClasses
{
    public static class TimestampExtensions
    {
        public static Timestamp Null => NullValue;

        private static readonly Timestamp NullValue = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));

        public static bool IsNull(this Timestamp timestamp)
        {
            return timestamp.Seconds == NullValue.Seconds && timestamp.Nanos == NullValue.Nanos;
        }

        public static Timestamp ToNullableTimestamp(this DateTime dateTime)
        {
            return Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        }

        public static Timestamp ToNullableTimestamp(this DateTime? dateTime)
        {
            if(dateTime.HasValue)
                return dateTime.Value.ToNullableTimestamp();
            return Null;
        }

        public static DateTime? ToNullableDateTime(this Timestamp timestamp)
        {
            return timestamp.IsNull() ? (DateTime?)null : timestamp.ToDateTime();
        }
    }
}