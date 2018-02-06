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

        public DomainUrlFilter(string domainPattern)
        {
            mDomainPattern = new Regex(domainPattern);
        }

        public IList<string> Filter(IList<string> urls)
        {
            return urls.Where(o => mDomainPattern.IsMatch(o)).ToList();
        }

        public async Task LoadCheckPoint()
        {
            return;
        }

        public async Task SaveCheckPoint()
        {
            return;
        }
    }
}
