using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaCommon.Helper;
using XiaoyaRanker.Config;

namespace XiaoyaRanker.VectorSpaceModelRanker
{
    public class VectorSpaceModelRanker : IRanker
    {
        protected RankerConfig mConfig;

        public VectorSpaceModelRanker(RankerConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<double> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var documentCount = mConfig.UrlFileStore.Count();

            foreach (var urlFileId in urlFileIds)
            {
                double score = 0;
                foreach (var word in words)
                {
                    var urlFile = mConfig.UrlFileStore.LoadById(urlFileId);
                    var urlFileIndexStat = mConfig.UrlFileIndexStatStore.LoadByWordInUrlFile(urlFileId, word);
                    if (urlFileIndexStat == null)
                    {
                        continue;
                    }
                    var wordFrequencyInDocument = urlFileIndexStat.WordFrequency;

                    var indexStat = mConfig.IndexStatStore.LoadByWord(word);
                    var documentFrequency = indexStat.DocumentFrequency;

                    var wordScore =
                        ScoringHelpers.TfIdf(wordFrequencyInDocument, documentFrequency, documentCount) / urlFile.Content.Length * 100;
                    score += wordScore;
                }
                yield return score;
            }
        }
    }
}
