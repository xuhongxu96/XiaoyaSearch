using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRanker.Config;

namespace XiaoyaRanker
{
    public class IntegratedRanker : IRanker
    {
        protected IRanker mQueryTermProximityRanker;
        protected IRanker mVectorSpaceModelRanker;

        public IntegratedRanker(RankerConfig config)
        {
            mQueryTermProximityRanker = new QueryTermProximityRanker.QueryTermProximityRanker(config);
            mVectorSpaceModelRanker = new VectorSpaceModelRanker.VectorSpaceModelRanker(config);
        }

        public IEnumerable<double> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var scores1 = mQueryTermProximityRanker.Rank(urlFileIds, words).ToList();
            var scores2 = mVectorSpaceModelRanker.Rank(urlFileIds, words).ToList();

            for (int i = 0; i < scores1.Count; ++i)
            {
                yield return scores1[i] + scores2[2];
            }
        }
    }
}
