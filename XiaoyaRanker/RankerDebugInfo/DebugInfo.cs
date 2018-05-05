using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker.RankerDebugInfo
{
    public class DebugInfo
    {
        public string RankerName { get; set; }
        private Dictionary<string, DebugInfoValue> mProperties = new Dictionary<string, DebugInfoValue>();
        public Dictionary<string, DebugInfoValue> Properties => mProperties;

        public DebugInfo(string rankerName)
        {
            RankerName = rankerName;
        }

        public DebugInfo(string rankerName, string propertyKey, string propertyValue)
        {
            RankerName = rankerName;

            mProperties[propertyKey] = new StringDebugInfoValue(propertyValue);
        }

        public override string ToString()
        {
            var textBuilder = new StringBuilder();
            foreach (var prop in Properties)
            {
                textBuilder.AppendFormat("{0}: {1}; ", prop.Key, prop.Value.ToString());
            }

            return string.Format("{0}: [ {1} ]", RankerName, textBuilder.ToString());
        }
    }
}
