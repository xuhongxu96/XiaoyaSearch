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
        protected List<IRanker> mRankers = new List<IRanker>();

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
            });

            var rankerConfig = new RankerConfig
            {
                IndexStatStore = config.IndexStatStore,
                UrlFileIndexStatStore = config.UrlFileIndexStatStore,
                UrlFileStore = config.UrlFileStore,
                InvertedIndexStore = config.InvertedIndexStore,
            };

            mRankers.Add(new VectorSpaceModelRanker(rankerConfig));
            mRankers.Add(new QueryTermProximityRanker(rankerConfig));
        }

        public IEnumerable<SearchResult> Search(string query)
        {
            var parsedQuery = mQueryParser.Parse(query);
            var urlFileIds = mRetriever.Retrieve(parsedQuery.Expression).ToList();
            var totalScores = new Dictionary<int, double>();

            foreach (var ranker in mRankers)
            {
                var scores = ranker.Rank(urlFileIds, parsedQuery.Words).ToList();
                for (int i = 0; i < urlFileIds.Count(); ++i)
                {
                    var id = urlFileIds[i];
                    if (totalScores.ContainsKey(id))
                    {
                        totalScores[id] += scores[i];
                    }
                    else
                    {
                        totalScores[id] = scores[i];
                    }
                }
            }

            urlFileIds.Sort((x, y) =>
            {
                return totalScores[y].CompareTo(totalScores[x]);
            });

            return from id in urlFileIds
                   select new SearchResult
                   {
                       UrlFileId = id,
                       Score = totalScores[id],
                   };
        }
    }
}
