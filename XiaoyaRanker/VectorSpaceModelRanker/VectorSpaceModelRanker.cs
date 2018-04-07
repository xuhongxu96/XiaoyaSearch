using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaCommon.Helper;
using XiaoyaRanker.Config;

namespace XiaoyaRanker.VectorSpaceModelRanker
{
    public class VectorSpaceModelRanker : IRanker
    {
        protected RankerConfig mConfig;
        protected const double ContentFactor = 1;
        protected const double TitleFactor = 10;

        public VectorSpaceModelRanker(RankerConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<double> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var documentCount = mConfig.UrlFileStore.Count();

            foreach (var urlFileId in urlFileIds)
            {
                var urlFile = mConfig.UrlFileStore.LoadById(urlFileId);
                var titleLength = urlFile.Title.Length + 1;
                var contentLength = urlFile.Content.Length + 1;

                double titleScore = 0;
                double score = 0;
                foreach (var word in words)
                {
                    var urlFileIndexStat = mConfig.UrlFileIndexStatStore.LoadByWordInUrlFile(urlFileId, word);

                    if (urlFileIndexStat == null)
                    {
                        continue;
                    }

                    var wordFrequencyInDocument = urlFileIndexStat.WordFrequency;

                    var indexStat = mConfig.IndexStatStore.LoadByWord(word);
                    var documentFrequency = indexStat.DocumentFrequency;

                    var wordScore =
                        ScoringHelpers.TfIdf(wordFrequencyInDocument, documentFrequency, documentCount);

                    if (urlFile.Title.Contains(word))
                    {
                        titleScore += wordScore * word.Length / titleLength;
                    }

                    score += wordScore * Math.Min(1, (0.3 + word.Length / contentLength));
                }
                yield return ContentFactor * score + TitleFactor * titleScore;
            }
        }
    }
}
