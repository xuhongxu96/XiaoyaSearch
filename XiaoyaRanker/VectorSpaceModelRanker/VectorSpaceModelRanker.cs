using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaCommon.Helper;
using XiaoyaRanker.Config;
using static XiaoyaStore.Data.Model.InvertedIndex;

namespace XiaoyaRanker.VectorSpaceModelRanker
{
    public class VectorSpaceModelRanker : IRanker
    {
        protected RankerConfig mConfig;
        protected const double ContentFactor = 1;
        protected const double TitleFactor = 10;
        protected const double LinkFactor = 5;

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

                double linkScore = 0;
                double titleScore = 0;
                double score = 0;
                foreach (var word in words)
                {
                    var urlFileIndexStat = mConfig.InvertedIndexStore.LoadByWordInUrlFile(urlFileId, word);

                    if (urlFileIndexStat == null)
                    {
                        continue;
                    }

                    var wordFrequencyInTitle =
                        mConfig.InvertedIndexStore.LoadByWordInUrlFile(urlFileId, word).OccurencesInTitle;
                    var wordFrequencyInLinks =
                        mConfig.InvertedIndexStore.LoadByWordInUrlFile(urlFileId, word).OccurencesInLinks;
                    var wordFrequencyInDocument = urlFileIndexStat.WordFrequency;

                    var indexStat = mConfig.IndexStatStore.LoadByWord(word);
                    var documentFrequency = indexStat.DocumentFrequency;

                    var wordScore =
                        ScoringHelper.TfIdf(wordFrequencyInDocument, documentFrequency, documentCount);

                    var titleWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInTitle, documentFrequency, documentCount);

                    var linkWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInLinks, documentFrequency, documentCount);

                    titleScore += titleWordScore * word.Length / titleLength;
                    linkScore += linkWordScore;

                    score += wordScore * Math.Min(1, (0.3 + word.Length / contentLength));
                }
                yield return ContentFactor * score + TitleFactor * titleScore + LinkFactor * linkScore;
            }
        }
    }
}
