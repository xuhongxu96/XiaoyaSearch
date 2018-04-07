using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCrawler.UrlFilter
{
    public class UrlNormalizer : IUrlFilter
    {
        public IEnumerable<string> Filter(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                var uri = new Uri(url);
                var result = uri.ToString();
                if (result.EndsWith("/#"))
                {
                    result = result.Substring(0, result.Length - 1);
                }
                yield return result;
            }
        }
    }
}
