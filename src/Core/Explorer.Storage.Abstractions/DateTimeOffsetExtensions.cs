using System;

namespace Explorer.Storage.Abstractions
{
    public static class DateTimeOffsetExtensions
    {
        public static long ToUnixTimeMicroseconds(this DateTimeOffset dateTimeOffset)
        {
            return (dateTimeOffset - DateTimeOffset.UnixEpoch).Ticks / 10;
        }
    }
}