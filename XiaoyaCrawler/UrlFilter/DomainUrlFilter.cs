using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XiaoyaCrawler.UrlFilter
{
    public class DomainUrlFilter : IUrlFilter
    {
        public IList<string> Filter(IList<string> urls)
        {
            return urls.Where(o => o.Contains("bnu.edu.cn")).ToList();
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
