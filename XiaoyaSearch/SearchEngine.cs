using System;
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
        }

        public IEnumerable<SearchResult> Search(string query)
        {
            var parsedQuery = mQueryParser.Parse(query);
            var urlFileIds = mRetriever.Retrieve(parsedQuery.Expression).ToList();
            var scores = mRanker.Rank(urlFileIds, parsedQuery.Words).ToList();

            var results = new List<SearchResult>();

            for(int i = 0; i < urlFileIds.Count; ++i)
            {
                results.Add(new SearchResult
                {
                    UrlFileId = urlFileIds[i],
                    Score = scores[i],
                });
            }

            return results.OrderByDescending(o => o.Score);
        }
    }
}
