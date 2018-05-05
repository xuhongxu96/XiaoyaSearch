using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRanker.Config;
using XiaoyaRanker.RankerDebugInfo;

namespace XiaoyaRanker.Ranker
{
    public class IntegratedRanker : IRanker
    {
        protected List<IRanker> mRankers;

        protected readonly List<double> Weights = new List<double>
        {
            1, 1
        };

        public IntegratedRanker(RankerConfig config)
        {
            mRankers = new List<IRanker>
            {
                new VectorSpaceModelRanker.VectorSpaceModelRanker(config),
                new DomainDepthRanker.DomainDepthRanker(config),
            };
        }

        public IEnumerable<Score> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var scores = new List<List<Score>>();
            foreach (var ranker in mRankers)
            {
                scores.Add(ranker.Rank(urlFileIds, words).ToList());
            }

            for (int i = 0; i < scores.First().Count; ++i)
            {
                double finalScore = 0;
                double weightSum = 0;
                var debugInfo = new DebugInfo(nameof(IntegratedRanker));

                for (int j = 0; j < scores.Count; ++j)
                {
                    finalScore += scores[j][i].Value * Weights[j];
                    weightSum += Weights[j];
                    debugInfo.Properties[scores[j][i].DebugInfo.RankerName] = new ScoreDebugInfoValue(scores[j][i]);
                }
                finalScore /= weightSum;

                yield return new Score
                {
                    Value = finalScore,
                    DebugInfo = debugInfo,
                };
            }
        }
    }
}
