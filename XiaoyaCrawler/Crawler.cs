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
using XiaoyaCrawler.SimilarContentManager;
using XiaoyaCrawler.UrlFilter;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaLogger;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler
{
    public class Crawler : ICrawler
    {
        public CrawlerStatus Status { get; private set; }

        protected CrawlerConfig mConfig;
        protected IUrlFrontier mUrlFrontier;
        protected IFetcher mFetcher;
        protected IParser mParser;
        protected ISimilarContentManager mSimilarContentJudger;
        protected List<IUrlFilter> mUrlFilters;

        protected SemaphoreSlim mFetchSemaphore;
        protected object mSyncLock = new object();

        protected CancellationTokenSource mCancellationTokenSource;
        protected ConcurrentBag<Task> mTasks = new ConcurrentBag<Task>();

        protected int mFetchCount;

        /// <summary>
        /// Logger
        /// </summary>
        protected RuntimeLogger mLogger;
        protected RuntimeLogger mErrorLogger;

        public Crawler(CrawlerConfig config,
            IUrlFrontier urlFrontier,
            IFetcher fetcher,
            IParser parser,
            ISimilarContentManager similarContentManager,
            List<IUrlFilter> urlFilters)
        {
            mConfig = config;
            Status = CrawlerStatus.STOPPED;
            mUrlFrontier = urlFrontier;
            mFetcher = fetcher;
            mParser = parser;
            mSimilarContentJudger = similarContentManager;
            mUrlFilters = urlFilters;
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.Log"), true);
            mErrorLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler Error.Log"), true);
        }

        protected void FetchUrlAsync(string url)
        {
            mFetchSemaphore.Wait();
            var t = Task.Run(() =>
            {
                mLogger.Log(nameof(Crawler), "Begin Crawl: " + url);
                try
                {
                    // Fetch Url
                    var urlFile = mFetcher.FetchAsync(url).GetAwaiter().GetResult();
                    if (urlFile == null)
                    {
                        // Fetched nothing
                        // Push back this url
                        mUrlFrontier.PushBackUrl(url, true);
                        return;
                    }

                    try
                    {
                        // Parse fetched file
                        var parseResult = mParser.ParseAsync(urlFile).GetAwaiter().GetResult();

                        // Store parsed content
                        urlFile.Content = parseResult.Content;

                        mConfig.UrlFileStore.Save(urlFile);

                        // Judge if there are other files that have similar content as this
                        mSimilarContentJudger.AddContentAsync(url, parseResult.Content);

                        // Filter urls
                        foreach (var filter in mUrlFilters)
                        {
                            parseResult.Urls = filter.Filter(parseResult.Urls);
                        }
                        // Add newly-found urls
                        foreach (var parsedUrl in parseResult.Urls)
                        {
                            mUrlFrontier.PushUrl(parsedUrl);
                        }
                        // Push back this url
                        mUrlFrontier.PushBackUrl(url);

                        mFetchCount++;
                    }
                    catch (NotSupportedException)
                    {
                        File.Delete(urlFile.FilePath);
                    }
                }
                catch (Exception e) when (
                    e is OperationCanceledException
                    || e is TaskCanceledException
                )
                {
                    mUrlFrontier.PushBackUrl(url);
                }
                catch (Exception e)
                {
                    mLogger.LogException(nameof(Crawler), "Failed to crawl url: " + url, e);
                    mErrorLogger.LogException(nameof(Crawler), "Failed to crawl url: " + url, e);
                    
                    // Retry
                    mUrlFrontier.PushBackUrl(url, true);
                }
                finally
                {
                    mLogger.Log(nameof(Crawler), "End Crawl: " + url);
                    mFetchSemaphore.Release();
                }
            });
            mTasks.Add(t);
        }

        public async Task StartAsync(bool restart = false)
        {
            mLogger.Log(nameof(Crawler), "Running");
            lock (mSyncLock)
            {
                if (Status == CrawlerStatus.RUNNING)
                {
                    return;
                }
                Status = CrawlerStatus.RUNNING;
            }

            if (restart)
            {
                mUrlFrontier = new SimpleUrlFrontier(mConfig);
            }

            mTasks.Clear();
            mFetchCount = 0;
            mFetchSemaphore = new SemaphoreSlim(mConfig.MaxFetchingConcurrency);

            mCancellationTokenSource = new CancellationTokenSource();
            var token = mCancellationTokenSource.Token;
            await Task.Run(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    UrlFrontierItem url = null;

                    try
                    {
                       url = mUrlFrontier.PopUrl();
                    }
                    catch (Exception e)
                    {
                        mLogger.LogException(nameof(Crawler), "Failed to pop url", e);
                        mErrorLogger.LogException(nameof(Crawler), "Failed to pop url", e);
                    }

                    if (url != null)
                    {
                        FetchUrlAsync(url.Url);
                    }
                    else if (mTasks.Any(o => !o.IsCompleted))
                    {
                        Task.WaitAny(mTasks.ToArray());
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
            }, token);
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

            mLogger.Log(nameof(Crawler), "Stopping");

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

            mLogger.Log(nameof(Crawler), "Stopped");
        }
    }
}
