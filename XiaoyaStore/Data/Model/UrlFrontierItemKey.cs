using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public struct UrlFrontierItemKey : IComparable<UrlFrontierItemKey>
    {
        private (DateTime, long) key;

        public UrlFrontierItemKey(DateTime plannedTime, long priority)
        {
            key = (plannedTime, priority);
        }

        public int CompareTo(UrlFrontierItemKey other)
        {
            return key.CompareTo(other.key);
        }
    }
}
