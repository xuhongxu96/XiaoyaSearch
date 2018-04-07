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
        protected const double TitleFactor = 10.0;

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
                    var titleIndex = mConfig.InvertedIndexStore.LoadByWordInUrlFileOrderByPosition(urlFileId, word, XiaoyaStore.Data.Model.InvertedIndex.InvertedIndexType.Title);

                    if (urlFileIndexStat == null)
                    {
                        continue;
                    }
                    var wordFrequencyInDocument = urlFileIndexStat.WordFrequency;

                    var indexStat = mConfig.IndexStatStore.LoadByWord(word);
                    var documentFrequency = indexStat.DocumentFrequency;

                    var wordScore =
                        ScoringHelpers.TfIdf(wordFrequencyInDocument, documentFrequency, documentCount) / urlFile.Content.Length * 100;

                    if (titleIndex != null)
                    {
                        wordScore *= TitleFactor;
                    }

                    score += wordScore;
                }
                yield return score;
            }
        }
    }
}
