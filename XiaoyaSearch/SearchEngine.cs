using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaLogger;
using XiaoyaQueryParser.Config;
using XiaoyaQueryParser.QueryParser;
using XiaoyaRanker;
using XiaoyaRanker.Config;
using XiaoyaRanker.PositionRanker;
using XiaoyaRanker.PositionRanker.QueryTermProximityRanker;
using XiaoyaRanker.Ranker;
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
        protected IPositionRanker mProRanker;
        protected SearchEngineConfig mConfig;

        protected RuntimeLogger mLogger;

        protected const int ResultSize = 150;
        protected const int PageSize = 150;

        protected static volatile int SessionId = 0;

        public SearchEngine(SearchEngineConfig config)
        {
            mConfig = config;
            mQueryParser = new SimpleQueryParser(new QueryParserConfig
            {
                TextSegmenter = config.TextSegmenter,
            });

            mRetriever = new InexactTopKRetriever(new RetrieverConfig
            {
                PostingListStore = config.PostingListStore,
                UrlFileStore = config.UrlFileStore,
                InvertedIndexStore = config.InvertedIndexStore,
            }, ResultSize);

            var rankerConfig = new RankerConfig
            {
                PostingListStore = config.PostingListStore,
                UrlFileStore = config.UrlFileStore,
                InvertedIndexStore = config.InvertedIndexStore,
            };

            mRanker = new IntegratedRanker(rankerConfig);
            mProRanker = new QueryTermProximityRanker(rankerConfig);

            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "SearchEngine.Log"), true);
        }

        public IEnumerable<SearchResult> Search(string query)
        {
            if (query == null || query.Trim() == "")
            {
                yield break;
            }
            query = query.Trim();

            var session = SessionId++;

            mLogger.Log(nameof(SearchEngine), session + "\tSearching: " + query);

            var time = DateTime.Now;

            var parsedQuery = mQueryParser.Parse(query);

            mLogger.Log(nameof(SearchEngine), session + "\tParsed query " + (DateTime.Now - time).TotalMilliseconds);
            time = DateTime.Now;

            var urlFileIds = mRetriever.Retrieve(parsedQuery.Expression).ToList();
            var count = urlFileIds.Count;

            mLogger.Log(nameof(SearchEngine), session + "\tRetrieved docs " + (DateTime.Now - time).TotalMilliseconds);
            time = DateTime.Now;

            var scores = mRanker.Rank(urlFileIds, parsedQuery.Words).ToList();

            mLogger.Log(nameof(SearchEngine), session + "\tRanked docs 1 " + (DateTime.Now - time).TotalMilliseconds);
            time = DateTime.Now;

            var results = new List<SearchResult>();

            for (int i = 0; i < count; ++i)
            {
                results.Add(new SearchResult
                {
                    UrlFileId = urlFileIds[i],
                    Score = scores[i],
                });
            }

            results = results.OrderByDescending(o => o.Score.Value).ToList();

            for (int i = 0; i < (PageSize - 1 + count) / PageSize; ++i)
            {
                int subResultsLength = Math.Min(PageSize, count - PageSize * i);
                var subResults = results.GetRange(i * PageSize, subResultsLength);

                var proScores = mProRanker.Rank(subResults.Select(o => o.UrlFileId), parsedQuery.Words).ToList();

                for (int j = 0; j < subResultsLength; ++j)
                {
                    subResults[j].ProScore = proScores[j];
                    subResults[j].WordPositions = proScores[j].WordPositions?.Where(o => o.Position != uint.MaxValue);
                }

                mLogger.Log(nameof(SearchEngine), session + "\tRanked docs 2 " + (DateTime.Now - time).TotalMilliseconds);
                time = DateTime.Now;

                foreach (var proResult in subResults.OrderByDescending(o => o.ProScore.Value))
                {
                    yield return proResult;
                }
            }
        }
    }
}
