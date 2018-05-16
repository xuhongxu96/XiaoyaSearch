using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public static class BondTypeAliasConverter
    {
        public static long Convert(DateTime value, long unused)
        {
            return value.ToUniversalTime().Ticks;
        }

        public static DateTime Convert(long value, DateTime unused)
        {
            return new DateTime(value, DateTimeKind.Utc);
        }

        public static long Convert(TimeSpan value, long unused)
        {
            return value.Ticks;
        }

        public static TimeSpan Convert(long value, TimeSpan unused)
        {
            return TimeSpan.FromTicks(value);
        }
    }
}
