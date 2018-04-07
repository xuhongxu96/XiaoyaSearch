using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRanker.Config;
using XiaoyaStore.Helper;

namespace XiaoyaRanker.DomainDepthRanker
{
    public class DomainDepthRanker : IRanker
    {
        protected RankerConfig mConfig;

        public DomainDepthRanker(RankerConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<double> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            foreach(var id in urlFileIds)
            {
                var urlFile = mConfig.UrlFileStore.LoadById(id);
                yield return Math.Exp(-UrlHelper.GetDomainDepth(urlFile.Url));
            }
        }
    }
}
