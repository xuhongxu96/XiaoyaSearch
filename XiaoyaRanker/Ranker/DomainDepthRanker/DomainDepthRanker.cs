using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRanker.Config;
using XiaoyaRanker.RankerDebugInfo;
using XiaoyaStore.Helper;

namespace XiaoyaRanker.Ranker.DomainDepthRanker
{
    public class DomainDepthRanker : IRanker
    {
        protected RankerConfig mConfig;

        public DomainDepthRanker(RankerConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<Score> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            foreach (var id in urlFileIds)
            {
                var urlFile = mConfig.UrlFileStore.LoadById(id);
                if (urlFile == null)
                {
                    yield return new Score
                    {
                        Value = 0,
                        DebugInfo = new DebugInfo(nameof(DomainDepthRanker),
                            "Error", "UrlFile Not Found"),
                    };
                }
                else
                {
                    var depth = UrlHelper.GetDomainDepth(urlFile.Url);
                    yield return new Score
                    {
                        Value = Math.Exp(-depth),
                        DebugInfo = new DebugInfo(nameof(DomainDepthRanker),
                            "DomainDepth", depth.ToString()),
                    };
                }
            }
        }
    }
}
