using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
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
        protected CancellationTokenSource mCancellationTokenSource;

        private SemaphoreSlim mFetchSemaphore;
        private object mStatusSyncLock = new object();
        private object mSaveSyncLock = new object();
        private ConcurrentDictionary<Task, bool> mTasks = new ConcurrentDictionary<Task, bool>();

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
            mErrorLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler Error.Log"), false);
        }

        protected void FetchUrlAsync(string url)
        {
            mFetchSemaphore.Wait();
            mLogger.Log(nameof(Crawler), "Begin Crawl: " + url, true);
            var t = Task.Run(() =>
            {
                FetchedFile fetchedFile = null;
                try
                {
                    // Fetch Url
                    fetchedFile = mFetcher.FetchAsync(url).GetAwaiter().GetResult();
                    if (fetchedFile == null)
                    {
                        // Fetched nothing
                        // Push back this url
                        mUrlFrontier.PushBackUrl(url, true);
                        return;
                    }

                    // Parse fetched file
                    var parseResult = mParser.ParseAsync(fetchedFile).GetAwaiter().GetResult();

                    var linkList = parseResult.Links;
                    // Filter urls
                    foreach (var filter in mUrlFilters)
                    {
                        linkList = filter.Filter(linkList).ToList();
                    }
#if DEBUG
                    var time = DateTime.Now;
#endif
                    lock (mSaveSyncLock)
                    {
                        // Judge if there are other files that have similar content as this
                        var (sameUrl, sameContent) = mSimilarContentJudger.JudgeContent(fetchedFile, parseResult.TextContent);
#if DEBUG
                        Console.WriteLine("Judged Similar: " + url + "\n" + (DateTime.Now - time).TotalSeconds);
                        time = DateTime.Now;
#endif
                        if (sameUrl != null)
                        {
                            return;
                        }
                    }

                    // TODO: Save UrlFile

#if DEBUG
                    Console.WriteLine("Saved UrlFile: " + url + "\n" + (DateTime.Now - time).TotalSeconds);
                    time = DateTime.Now;
#endif

                    // TODO: Save links

#if DEBUG
                    time = DateTime.Now;
#endif
                    var urls = linkList.Select(o => o.Url).Distinct();
                    // Add newly-found urls
                    mUrlFrontier.PushUrls(urls);

                    // Push back this url
                    mUrlFrontier.PushBackUrl(url);
#if DEBUG
                    Console.WriteLine("Pushed Links: " + url + "\n" + (DateTime.Now - time).TotalSeconds);
                    time = DateTime.Now;
#endif

                    mLogger.Log(nameof(Crawler), "End Crawl: " + url);
                }
                catch (NotSupportedException e)
                {
                    mLogger.LogException(nameof(Crawler), "Not supported file format: " + url, e, false);
                    mErrorLogger.LogException(nameof(Crawler), "Not supported file format: " + url, e);

                    // Retry
                    mUrlFrontier.PushBackUrl(url, true);
                }
                catch (InvalidDataException e)
                {
                    mLogger.LogException(nameof(Crawler), "Invalid data: " + url, e);
                    mErrorLogger.LogException(nameof(Crawler), "Invalid data: " + url, e);

                    // Retry
                    mUrlFrontier.PushBackUrl(url, true);
                }
                catch (UriFormatException e)
                {
                    mLogger.LogException(nameof(Crawler), "Invalid Uri: " + url, e);
                    mErrorLogger.LogException(nameof(Crawler), "Invalid Uri: " + url, e);

                    mUrlFrontier.RemoveUrl(url);
                }
                catch (IOException e)
                {
                    mLogger.LogException(nameof(Crawler), "Failed to fetch: " + url, e, false);
                    mErrorLogger.LogException(nameof(Crawler), "Failed to fetch: " + url, e);

                    mUrlFrontier.RemoveUrl(url);
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
                    mFetchSemaphore.Release();
                    if (fetchedFile != null && File.Exists(fetchedFile.filePath))
                    {
                        File.Delete(fetchedFile.filePath);
                    }
                }
            }).ContinueWith(task =>
                {
                    mTasks.TryRemove(task, out bool v);
                });
            mTasks.TryAdd(t, true);
        }

        public async Task StartAsync(bool restart = false)
        {
            mLogger.Log(nameof(Crawler), "Running");
            lock (mStatusSyncLock)
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

                    string url = null;

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
                        FetchUrlAsync(url);
                    }
                    else if (mTasks.Keys.Any())
                    {
                        Task.WaitAny(mTasks.Keys.ToArray());
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
            lock (mStatusSyncLock)
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
                    Task.WaitAll(mTasks.Keys.ToArray());
                    lock (mStatusSyncLock)
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
