using System;
using System.Linq;

namespace XiaoyaStore.Helper
{
    public static class UrlHelper
    {
        public static string UrlToFileName(string url)
        {
            return HashHelper.GetStringMd5(url)
                + new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
        }

        public static string GetHost(string url)
        {
            try
            {
                var host = new Uri(url).Host;
                if (host.StartsWith("www."))
                {
                    host = host.Substring(4);
                }
                return host;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static int GetDomainDepth(string url)
        {
            try
            {
                var path = new Uri(url).PathAndQuery;
                var count = path.Count(o => o == '/');
                if (path.EndsWith("/")) count--;
                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
