﻿using System;
using System.Collections.Generic;
using System.Linq;
using XiaoyaQueryParser.Config;
using XiaoyaQueryParser.QueryParser;
using XiaoyaRanker;
using XiaoyaRanker.Config;
using XiaoyaRanker.QueryTermProximityRanker;
using XiaoyaRanker.VectorSpaceModelRanker;
using XiaoyaRetriever;
using XiaoyaRetriever.Config;
using XiaoyaRetriever.InexactTopKRetriever;
using XiaoyaSearch.Config;

namespace XiaoyaSearch
{
    public class SearchEngine
    {
        protected IQueryParser mQueryParser;
        protected IRetriever mRetriever;
        protected IRanker mRanker;
        protected IRanker mProRanker;

        protected const int PageSize = 20;

        public SearchEngine(SearchEngineConfig config)
        {
            mQueryParser = new SimpleQueryParser(new QueryParserConfig
            {
                TextSegmenter = config.TextSegmenter,
            });

            mRetriever = new InexactTopKRetriever(new RetrieverConfig
            {
                IndexStatStore = config.IndexStatStore,
                UrlFileIndexStatStore = config.UrlFileIndexStatStore,
                UrlFileStore = config.UrlFileStore,
                InvertedIndexStore = config.InvertedIndexStore,
            }, 300);

            var rankerConfig = new RankerConfig
            {
                IndexStatStore = config.IndexStatStore,
                UrlFileIndexStatStore = config.UrlFileIndexStatStore,
                UrlFileStore = config.UrlFileStore,
                InvertedIndexStore = config.InvertedIndexStore,
            };

            mRanker = new IntegratedRanker(rankerConfig);
            mProRanker = new QueryTermProximityRanker(rankerConfig);
        }

        public IEnumerable<SearchResult> Search(string query)
        {
            query = query.Trim();
            if (query == "")
            {
                yield break;
            }

            var parsedQuery = mQueryParser.Parse(query);

            var urlFileIds = mRetriever.Retrieve(parsedQuery.Expression).ToList();
            var count = urlFileIds.Count;

            var scores = mRanker.Rank(urlFileIds, parsedQuery.Words).ToList();

            var results = new List<SearchResult>();

            for (int i = 0; i < count; ++i)
            {
                results.Add(new SearchResult
                {
                    UrlFileId = urlFileIds[i],
                    Score = scores[i],
                });
            }

            results = results.OrderByDescending(o => o.Score).ToList();

            for (int i = 0; i < (PageSize - 1 + count) / PageSize; ++i)
            {
                int subResultsLength = Math.Min(PageSize, count - PageSize * i);
                var subResults = results.GetRange(i * PageSize, subResultsLength);
                var proScores = mProRanker.Rank(subResults.Select(o => o.UrlFileId), parsedQuery.Words).ToList();

                for (int j = 0; j < subResultsLength; ++j)
                {
                    subResults[j].ProScore = proScores[j];
                }

                foreach (var proResult in subResults.OrderByDescending(o => o.ProScore))
                {
                    yield return proResult;
                }
            }
        }
    }
}