﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaCommon.Helper;
using XiaoyaRanker.Config;
using XiaoyaRanker.RankerDebugInfo;

namespace XiaoyaRanker.Ranker.VectorSpaceModelRanker
{
    public class VectorSpaceModelRanker : IRanker
    {
        protected RankerConfig mConfig;
        protected const double ContentFactor = 1;
        protected const double TitleFactor = 10;
        protected const double LinkFactor = 20;
        protected const double HeaderFactor = 15;

        public VectorSpaceModelRanker(RankerConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<Score> Rank(IEnumerable<ulong> urlFileIds, IEnumerable<string> words)
        {
            var documentCount = mConfig.UrlFileStore.GetCount();

            foreach (var urlFileId in urlFileIds)
            {
                var urlFile = mConfig.UrlFileStore.GetUrlFile(urlFileId);
                if (urlFile == null)
                {
                    yield return new Score
                    {
                        Value = 0,
                        DebugInfo = new DebugInfo(nameof(VectorSpaceModelRanker), 
                            "Error", "UrlFile Not Found"),
                    };
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
                    var urlFileIndexStat = mConfig.InvertedIndexStore.GetIndex(urlFileId, word);

                    if (urlFileIndexStat == null)
                    {
                        continue;
                    }

                    var wordFrequencyInTitle = urlFileIndexStat.OccurencesInTitle;
                    var wordFrequencyInLinks = urlFileIndexStat.OccurencesInLinks;
                    var wordFrequencyInHeaders = urlFileIndexStat.OccurencesInHeaders;
                    var wordFrequencyInDocument = urlFileIndexStat.WordFrequency;

                    var postingList = mConfig.PostingListStore.GetPostingList(word);
                    var documentFrequency = postingList.DocumentFrequency;

                    var wordScore = 
                        ScoringHelper.TfIdf(wordFrequencyInDocument, documentFrequency, documentCount);

                    var titleWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInTitle, documentFrequency, documentCount);

                    var linkWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInLinks, documentFrequency, documentCount);

                    var headerWordScore =
                        ScoringHelper.TfIdf(wordFrequencyInHeaders, documentFrequency, documentCount);

                    titleScore += titleWordScore * Math.Min(1, (0.3 + word.Length / titleLength));
                    linkScore += linkWordScore / urlFile.InLinkCount; // * word.Length / (1 + urlFile.LinkTotalLength);
                    headerScore += headerWordScore; // * word.Length / (1 + urlFile.HeaderTotalLength);
                    contentScore += wordScore * Math.Min(1, (0.3 + word.Length / contentLength));
                }
                var finalScore = (ContentFactor * contentScore
                    + TitleFactor * titleScore 
                    + LinkFactor * linkScore
                    + HeaderFactor * headerScore) 
                    / (ContentFactor + TitleFactor + LinkFactor + HeaderFactor);

                var debugInfo = new DebugInfo(nameof(VectorSpaceModelRanker));
                debugInfo.Properties["ContentScore"] = new StringDebugInfoValue(contentScore.ToString());
                debugInfo.Properties["TitleScore"] = new StringDebugInfoValue(titleScore.ToString());
                debugInfo.Properties["HeaderScore"] = new StringDebugInfoValue(headerScore.ToString());
                debugInfo.Properties["LinkScore"] = new StringDebugInfoValue(linkScore.ToString());

                yield return new Score
                {
                    Value = finalScore,
                    DebugInfo = debugInfo,
                };
            }
        }
    }
}
