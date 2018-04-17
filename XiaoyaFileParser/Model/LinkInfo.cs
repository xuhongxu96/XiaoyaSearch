using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Helper;

namespace XiaoyaFileParser.Model
{
    public class LinkInfo
    {
        public string Url { get; set; }
        public string Text { get; set; }

        public string Host
        {
            get => UrlHelper.GetHost(Url);
        }

        public override bool Equals(object obj)
        {
            var info = obj as LinkInfo;
            return info != null &&
                   Text == info.Text &&
                   Host == info.Host;
        }

        public override int GetHashCode()
        {
            var hashCode = 2058120093;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Host);
            return hashCode;
        }
    }
}
