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

                if (uri.Query.Contains("?"))
                {
                    var exceptQuery = url.Substring(0, url.Length - uri.Query.Length);
                    var queries = HttpUtility.ParseQueryString(uri.Query);
                    var newQuery = new List<string>();
                    foreach (var key in queries.AllKeys.Distinct())
                    {
                        var value = queries.Get(key)
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .LastOrDefault();
                        newQuery.Add(key + "=" + value);
                    }

                    result = new Uri(exceptQuery + "?" + string.Join("&", newQuery)).ToString();
                }
                else
                {
                    result = new Uri(url).ToString();
                }

                if (result.EndsWith("#") || result.EndsWith("/"))
                {
                    result = result.Substring(0, result.Length - 1);
                }

                yield return result;
            }
        }
    }
}
