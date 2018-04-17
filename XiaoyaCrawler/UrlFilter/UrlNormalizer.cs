using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace XiaoyaCrawler.UrlFilter
{
    public class UrlNormalizer : IUrlFilter
    {
        public IEnumerable<string> Filter(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                var uri = new Uri(url);
                var result = url;

                var exceptQuery = url.Substring(0, url.Length - uri.Query.Length);
                var queries = HttpUtility.ParseQueryString(uri.Query);
                var newQuery = "?";
                foreach (var key in queries.AllKeys.Distinct())
                {
                     newQuery += key + "=" + queries.Get(key);
                }

                result = new Uri(exceptQuery + newQuery).ToString();

                yield return result;
            }
        }
    }
}
