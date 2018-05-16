using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.UrlFilter
{
    public class DomainUrlFilter : IUrlFilter
    {

        private Regex mDomainPattern;
        private Regex mBlackPattern = null;

        public DomainUrlFilter(string domainPattern, string blackPattern = null)
        {
            mDomainPattern = new Regex(domainPattern, RegexOptions.Compiled);
            if (blackPattern != null)
            {
                mBlackPattern = new Regex(blackPattern, RegexOptions.Compiled);
            }
        }

        public IEnumerable<LinkInfo> Filter(IEnumerable<LinkInfo> links)
        {
            if (mBlackPattern == null)
            {
                return links.Where(o => mDomainPattern.IsMatch(o.Url));
            }
            else
            {
                return links.Where(o => mDomainPattern.IsMatch(o.Url) && !mBlackPattern.IsMatch(o.Url));
            }
        }
    }
}
