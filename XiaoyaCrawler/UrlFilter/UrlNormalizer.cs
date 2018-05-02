using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.UrlFilter
{
    public class UrlNormalizer : IUrlFilter
    {
        private readonly HashSet<string> UrlQueryKeyFilter = new HashSet<string>
        {
            "orderby", "order", "filter", "filterby", "referer", "reply", "replytocom"
        };

        public IEnumerable<Link> Filter(IEnumerable<Link> links)
        {
            foreach (var link in links)
            {
                string result;

                try
                {
                    var normUrl = Uri.EscapeUriString(link.Url);
                    var uri = new Uri(normUrl);

                    if (uri.Scheme != "http" && uri.Scheme != "https")
                    {
                        continue;
                    }

                    if (uri.Query.Contains("?") && uri.Query.Contains("="))
                    {
                        var exceptQueryAndFragment = normUrl.Substring(0, normUrl.Length - uri.Query.Length - uri.Fragment.Length);
                        var queries = HttpUtility.ParseQueryString(uri.Query);
                        var newQuery = new List<string>();
                        foreach (var key in queries.AllKeys.Distinct())
                        {
                            if (UrlQueryKeyFilter.Contains(key))
                            {
                                continue;
                            }
                            var value = queries.Get(key)
                                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .LastOrDefault();
                            newQuery.Add(key + "=" + value);
                        }

                        result = new Uri(exceptQueryAndFragment + "?" + string.Join("&", newQuery)).ToString();
                    }
                    else if (uri.Fragment.Any())
                    {
                        result = new Uri(normUrl.Substring(0, normUrl.Length - uri.Fragment.Length)).ToString();
                    }
                    else
                    {
                        result = new Uri(normUrl).ToString();
                    }

                }
                catch (UriFormatException)
                {
                    continue;
                }

                yield return new Link
                {
                    LinkId = link.LinkId,
                    UrlFileId = link.UrlFileId,
                    Text = link.Text,
                    Url = result,
                };

            }
        }
    }
}
