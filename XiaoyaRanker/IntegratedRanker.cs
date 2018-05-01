using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRanker.Config;

namespace XiaoyaRanker
{
    public class IntegratedRanker : IRanker
    {
        protected IRanker mVectorSpaceModelRanker;
        protected IRanker mDomainDepthRanker;
        // protected IRanker mQueryTermProximityRanker;

        public IntegratedRanker(RankerConfig config)
        {
            mVectorSpaceModelRanker = new VectorSpaceModelRanker.VectorSpaceModelRanker(config);
            mDomainDepthRanker = new DomainDepthRanker.DomainDepthRanker(config);
            // mQueryTermProximityRanker = new QueryTermProximityRanker.QueryTermProximityRanker(config);            
        }

        public IEnumerable<double> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var scores1 = mVectorSpaceModelRanker.Rank(urlFileIds, words).ToList();
            var scores2 = mDomainDepthRanker.Rank(urlFileIds, words).ToList();
            // var scores3 = mQueryTermProximityRanker.Rank(urlFileIds, words).ToList();

            for (int i = 0; i < scores1.Count; ++i)
            {
                yield return (scores1[i] / 10 + 5 * scores2[i]) / 6 /*+ scores3[i]*/;
            }
        }
    }
}
