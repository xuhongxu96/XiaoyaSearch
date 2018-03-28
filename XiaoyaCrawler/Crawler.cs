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
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.Log"));
            mErrorLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Error.Log"));
        }

        protected async void FetchUrlAsync(string url)
        {
            var t = Task.Run(() =>
            {
                mLogger.Log(nameof(Crawler), "Begin Crawl: " + url);
                try
                {
                    // Fetch Url
                    var urlFile = mFetcher.FetchAsync(url).GetAwaiter().GetResult();
                    if (urlFile == null)
                    {
                        // Fetched nothing, return.
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
                        if (!parseResult.Urls.Contains(url))
                        {
                            mUrlFrontier.PushBackUrl(url);
                        }

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
                    mLogger.Log(nameof(Crawler),
                        url + " Error\r\n" + e.Message + "\r\n---\r\n" + e.StackTrace);
                    mErrorLogger.Log(nameof(Crawler), 
                        url + " Error\r\n" + e.Message + "\r\n---\r\n" + e.StackTrace);
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
            await t;
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
                while (!mUrlFrontier.IsEmpty)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var url = mUrlFrontier.PopUrl();
                    if (url != null)
                    {
                        mFetchSemaphore.Wait();
                        FetchUrlAsync(url.Url);
                    }
                    while (mUrlFrontier.IsEmpty && mTasks.Any(o => !o.IsCompleted))
                    {
                        Task.WaitAny(mTasks.ToArray());
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
