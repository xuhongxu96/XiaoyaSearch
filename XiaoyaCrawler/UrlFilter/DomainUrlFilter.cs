using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public IEnumerable<string> Filter(IEnumerable<string> urls)
        {
            if (mBlackPattern == null)
            {
                return urls.Where(o => mDomainPattern.IsMatch(o));
            }
            else
            {
                return urls.Where(o => mDomainPattern.IsMatch(o) && !mBlackPattern.IsMatch(o));
            }
        }
    }
}
