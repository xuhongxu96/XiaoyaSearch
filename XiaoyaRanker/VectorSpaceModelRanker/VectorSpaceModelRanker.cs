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
        protected const double TitleFactor = 5;
        protected const double LinkFactor = 20;
        protected const double HeaderFactor = 10;

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
                if (urlFile == null)
                {
                    yield return 0;
                    continue;
                }
                var titleLength = urlFile.Title.Length + 1;
                var contentLength = urlFile.TextContent.Length + 1;

                double headerScore = 0;
                double linkScore = 0;
                double titleScore = 0;
                double contentScore = 0;

                foreach (var word in words)
                {
                    var urlFileIndexStat = mConfig.InvertedIndexStore.LoadByWordInUrlFile(urlFileId, word);

                    if (urlFileIndexStat == null)
                    {
                        continue;
                    }

                    var wordFrequencyInTitle = urlFileIndexStat.OccurencesInTitle;
                    var wordFrequencyInLinks = urlFileIndexStat.OccurencesInLinks;
                    var wordFrequencyInHeaders = urlFileIndexStat.OccurencesInHeaders;
                    var wordFrequencyInDocument = urlFileIndexStat.WordFrequency;

                    var indexStat = mConfig.IndexStatStore.LoadByWord(word);
                    var documentFrequency = indexStat.DocumentFrequency;

                    var wordScore = 
                        ScoringHelper.TfIdf(wordFrequencyInDocument, documentFrequency, documentCount);

                    var titleWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInTitle, documentFrequency, documentCount);

                    var linkWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInLinks, documentFrequency, documentCount);

                    var headerWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInHeaders, documentFrequency, documentCount);

                    titleScore += titleWordScore * word.Length / titleLength;
                    linkScore += linkWordScore * word.Length / (1 + urlFile.LinkTotalLength);
                    headerScore += headerWordScore * word.Length / (1 + urlFile.HeaderTotalLength);
                    contentScore += wordScore * Math.Min(1, (0.3 + word.Length / contentLength));
                }
                yield return (ContentFactor * contentScore
                    + TitleFactor * titleScore 
                    + LinkFactor * linkScore
                    + HeaderFactor * headerScore) 
                    / (ContentFactor + TitleFactor + LinkFactor + HeaderFactor);
            }
        }
    }
}
