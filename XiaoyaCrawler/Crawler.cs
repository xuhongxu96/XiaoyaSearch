using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Fetcher;
using XiaoyaCrawler.Parser;
using XiaoyaCrawler.SimilarContentJudger;
using XiaoyaCrawler.UrlFilter;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaLogger;

namespace XiaoyaCrawler
{
    public class Crawler : ICrawler
    {
        public CrawlerStatus Status { get; private set; }

        protected CrawlerConfig mConfig;
        protected IUrlFrontier mUrlFrontier;
        protected IFetcher mFetcher;
        protected IParser mParser;
        protected ISimilarContentJudger mSimilarContentJudger;
        protected List<IUrlFilter> mUrlFilters;

        protected SemaphoreSlim mFetchSemaphore;
        protected object mSyncLock = new object();

        protected CancellationTokenSource mCancellationTokenSource;
        protected List<Task> mTasks = new List<Task>();

        protected ConcurrentDictionary<string, int> mRetriedUrlMap;

        protected int mFetchCount;

        /// <summary>
        /// Logger
        /// </summary>
        protected RuntimeLogger mLogger;

        public Crawler(CrawlerConfig config)
        {
            mConfig = config;
            Status = CrawlerStatus.STOPPED;
            mUrlFrontier = new SimpleUrlFrontier(config);
            mFetcher = new SimpleFetcher(config);
            mParser = new SimpleParser(config);
            mSimilarContentJudger = new SimpleSimilarContentJudger(config);
            mUrlFilters = new List<IUrlFilter>
            {
                new DomainUrlFilter(),
                new DuplicateUrlEliminator(config),
            };
            mRetriedUrlMap = new ConcurrentDictionary<string, int>();
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.Log"));
        }

        protected async void FetchUrlAsync(string url)
        {
            var t = Task.Run(async () =>
            {
                try
                {
                    var urlFile = await mFetcher.FetchAsync(url);
                    if (urlFile == null)
                    {
                        return;
                    }
                    var parseResult = await mParser.ParseAsync(urlFile);
                    if (parseResult == null)
                    {
                        File.Delete(urlFile.FilePath);
                        return;
                    }
                    mSimilarContentJudger.AddContentAsync(url, parseResult.Content);
                    foreach (var filter in mUrlFilters)
                    {
                        parseResult.Urls = filter.Filter(parseResult.Urls);
                    }
                    foreach (var parsedUrl in parseResult.Urls)
                    {
                        mUrlFrontier.PushUrl(parsedUrl);
                    }
                    mFetchCount++;
                    if (mFetchCount % 50 == 0)
                    {
                        await SaveCheckPointsAsync();
                    }
                }
                catch (Exception e)
                {
                    mLogger.Log(nameof(Crawler), e.Message + "\n---\n" + e.StackTrace);
                    // Retry
                    if (mRetriedUrlMap.GetValueOrDefault(url, 0) < 3)
                    {
                        mRetriedUrlMap.AddOrUpdate(url, 1, (_url, oldVal) => oldVal + 1);
                        mUrlFrontier.PushUrl(url);
                    }
                }
                finally
                {
                    mFetchSemaphore.Release();
                }
            });
            mTasks.Add(t);
            await t;
        }

        private async Task LoadCheckPointsAsync()
        {
            mLogger.Log(nameof(Crawler), "Load Check Points: Begin");
            await Task.Run(() =>
            {
                var loadCheckPointTasks = new List<Task>
                {
                    mUrlFrontier.LoadCheckPoint(),
                    mSimilarContentJudger.LoadCheckPoint()
                };
                foreach (var urlFilter in mUrlFilters)
                {
                    loadCheckPointTasks.Add(urlFilter.LoadCheckPoint());
                }
                Task.WaitAll(loadCheckPointTasks.ToArray());
            });
            mLogger.Log(nameof(Crawler), "Load Check Points: End");
        }

        private async Task SaveCheckPointsAsync()
        {
            mLogger.Log(nameof(Crawler), "Save Check Points: Begin");
            await Task.Run(() =>
            {
                var saveCheckPointTasks = new List<Task>
                {
                    mUrlFrontier.SaveCheckPoint(),
                    mSimilarContentJudger.SaveCheckPoint()
                };
                foreach (var urlFilter in mUrlFilters)
                {
                    saveCheckPointTasks.Add(urlFilter.SaveCheckPoint());
                }
                Task.WaitAll(saveCheckPointTasks.ToArray());
            });
            mLogger.Log(nameof(Crawler), "Save Check Points: End");
        }

        public async Task StartAsync(bool restart = false)
        {
            lock (mSyncLock)
            {
                if (Status == CrawlerStatus.RUNNING)
                {
                    return;
                }
                else if (Status == CrawlerStatus.FINISHED)
                {
                    restart = true;
                }
                Status = CrawlerStatus.RUNNING;
            }

            if (restart)
            {
                mUrlFrontier = new SimpleUrlFrontier(mConfig);
            }
            else
            {
                await LoadCheckPointsAsync();
            }

            mTasks.Clear();
            mFetchCount = 0;
            mFetchSemaphore = new SemaphoreSlim(mConfig.MaxFetchingConcurrency);

            mCancellationTokenSource = new CancellationTokenSource();
            var ct = mCancellationTokenSource.Token;
            await Task.Run(() =>
            {
                while (!mUrlFrontier.IsEmpty)
                {
                    ct.ThrowIfCancellationRequested();

                    var url = mUrlFrontier.PopUrl();
                    mFetchSemaphore.Wait();
                    FetchUrlAsync(url);

                    while (mUrlFrontier.IsEmpty && mTasks.Any(o => !o.IsCompleted))
                    {
                        Task.WaitAny(mTasks.ToArray());
                    }
                }
            }, ct);

            lock (mSyncLock)
            {
                Status = CrawlerStatus.FINISHED;
            }
        }

        public async Task StopAsync()
        {
            lock (mSyncLock)
            {
                if (Status != CrawlerStatus.RUNNING)
                {
                    return;
                }
            }

            mCancellationTokenSource.Cancel();
            await Task.Run(() =>
            {
                try
                {
                    Task.WaitAll(mTasks.ToArray());
                    lock (mSyncLock)
                    {
                        Status = CrawlerStatus.STOPPED;
                    }
                }
                finally
                {
                    mCancellationTokenSource.Dispose();
                }
            });
        }
    }
}
