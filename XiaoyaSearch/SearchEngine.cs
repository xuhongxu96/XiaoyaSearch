using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        protected SearchEngineConfig mConfig;

        protected const int ResultSize = 500;
        protected const int PageSize = 500;

        public SearchEngine(SearchEngineConfig config)
        {
            mConfig = config;
            mQueryParser = new SimpleQueryParser(new QueryParserConfig
            {
                TextSegmenter = config.TextSegmenter,
            });

            mRetriever = new InexactTopKRetriever(new RetrieverConfig
            {
                IndexStatStore = config.IndexStatStore,
                UrlFileStore = config.UrlFileStore,
                InvertedIndexStore = config.InvertedIndexStore,
            }, ResultSize);

            var rankerConfig = new RankerConfig
            {
                IndexStatStore = config.IndexStatStore,
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

            Console.WriteLine("Begin search");
            var time = DateTime.Now;

            var parsedQuery = mQueryParser.Parse(query);

            Console.WriteLine("Parsed query " + (DateTime.Now - time).TotalMilliseconds);
            time = DateTime.Now;

            var urlFileIds = mRetriever.Retrieve(parsedQuery.Expression).ToList();
            var count = urlFileIds.Count;

            Console.WriteLine("Retrieved docs " + (DateTime.Now - time).TotalMilliseconds);
            time = DateTime.Now;

            Task.WaitAll(new Task[] {
                Task.Run(() =>
                {
                    var innerTime = DateTime.Now;
                    mConfig.InvertedIndexStore.CacheWordsInUrlFiles(urlFileIds, parsedQuery.Words);
                    Console.WriteLine("Cached words " + (DateTime.Now - innerTime).TotalMilliseconds);
                }),
                Task.Run(() =>
                {
                    var innerTime = DateTime.Now;
                    mConfig.UrlFileStore.CacheUrlFiles(urlFileIds);
                    Console.WriteLine("Cached docs " + (DateTime.Now - innerTime).TotalMilliseconds);
                }),
            });

            Console.WriteLine("Cached all " + (DateTime.Now - time).TotalMilliseconds);
            time = DateTime.Now;

            var scores = mRanker.Rank(urlFileIds, parsedQuery.Words).ToList();

            Console.WriteLine("Ranked docs 1 " + (DateTime.Now - time).TotalMilliseconds);
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

                Console.WriteLine("Ranked docs 2 " + (DateTime.Now - time).TotalMilliseconds);
                time = DateTime.Now;

                foreach (var proResult in subResults.OrderByDescending(o => o.ProScore))
                {
                    yield return proResult;
                }
            }
        }
    }
}
