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
using XiaoyaCommon.Helper;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Fetcher;
using XiaoyaCrawler.Parser;
using XiaoyaCrawler.SimilarContentManager;
using XiaoyaCrawler.UrlFilter;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaFileParser;
using XiaoyaFileParser.Model;
using XiaoyaLogger;
using XiaoyaStore.Model;

namespace XiaoyaCrawler
{
    public class Crawler : ICrawler
    {
        public CrawlerStatus Status { get; private set; }

        protected CrawlerConfig mConfig;
        protected IUrlFrontier mUrlFrontier;
        protected IFetcher mFetcher;
        protected ISimilarContentManager mSimilarContentJudger;
        protected List<IUrlFilter> mUrlFilters;
        protected CancellationTokenSource mCancellationTokenSource;

        protected Dictionary<string, string> mContentUrlDict
            = new Dictionary<string, string>();
        protected readonly object mContentUrlDictSyncLock = new object();

        private SemaphoreSlim mFetchSemaphore;
        private readonly object mStatusSyncLock = new object();
        private ConcurrentDictionary<Task, bool> mTasks = new ConcurrentDictionary<Task, bool>();

        /// <summary>
        /// Logger
        /// </summary>
        protected RuntimeLogger mLogger;
        protected RuntimeLogger mErrorLogger;

        public Crawler(CrawlerConfig config,
            IUrlFrontier urlFrontier,
            IFetcher fetcher,
            ISimilarContentManager similarContentManager,
            List<IUrlFilter> urlFilters)
        {
            mConfig = config;
            Status = CrawlerStatus.STOPPED;
            mUrlFrontier = urlFrontier;
            mFetcher = fetcher;
            mSimilarContentJudger = similarContentManager;
            mUrlFilters = urlFilters;
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.Log"), true);
            mErrorLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler Error.Log"), false);
        }

        private FetchedFile FetchUrl(string url)
        {
            var fetchedFile = mFetcher.FetchAsync(url).GetAwaiter().GetResult();
            if (fetchedFile == null)
            {
                // Fetched nothing
                // Push back this url
                mUrlFrontier.PushBackUrl(url, 0, true);
                return null;
            }
            return fetchedFile;
        }

        private (UrlFile urlFile, IList<LinkInfo> links, IList<Token> tokens)
            ParseFetchedFile(FetchedFile fetchedFile, IList<string> inLinkTexts)
        {
            UniversalFileParser parser = new UniversalFileParser();
            parser.SetFile(fetchedFile.MimeType, fetchedFile.Url, fetchedFile.Charset, fetchedFile.FilePath);

            var content = parser.GetContentAsync().GetAwaiter().GetResult();
            var textContent = parser.GetTextContentAsync().GetAwaiter().GetResult();
            var title = parser.GetTitleAsync().GetAwaiter().GetResult();
            var headers = parser.GetHeadersAsync().GetAwaiter().GetResult();
            var links = parser.GetLinksAsync().GetAwaiter().GetResult();
            var tokens = parser.GetTokensAsync(inLinkTexts).GetAwaiter().GetResult();

            var urlFile = new UrlFile
            {
                Charset = fetchedFile.Charset,
                Content = content,
                FileHash = fetchedFile.FileHash,
                FilePath = fetchedFile.FilePath,
                MimeType = fetchedFile.MimeType,
                TextContent = textContent,
                Title = title,
                Url = fetchedFile.Url,
                HeaderTotalLength = (uint)headers.Sum(o => o.Text.Length),
                HeaderCount = (uint)headers.Sum(o => (6 - o.Level)),
                InLinkCount = (uint)inLinkTexts.Count,
                InLinkTotalLength = (uint)inLinkTexts.Sum(o => o.Length),
                PageRank = 0.1,
            };

            return (urlFile, links, tokens);
        }

        private string GetSameUrl(FetchedFile fetchedFile, string content)
        {
            // Judge if there are other files that have similar content as this
            var (sameUrl, sameContent) = mSimilarContentJudger.JudgeContent(fetchedFile, content);

            lock (mContentUrlDictSyncLock)
            {
                if (mContentUrlDict.ContainsKey(content))
                {
                    return mContentUrlDict[content];
                }
                if (sameUrl == null)
                {
                    mContentUrlDict.Add(content, fetchedFile.Url);
                }
            }

            return sameUrl;
        }

        private IList<LinkInfo> FilterLinks(IList<LinkInfo> linkList)
        {
            // Filter urls
            foreach (var filter in mUrlFilters)
            {
                linkList = filter.Filter(linkList).ToList();
            }
            return linkList;
        }

        private IList<string> GetInLinkTexts(string url)
        {
            var inLinks = mConfig.LinkStore.GetLinks(url);

            return inLinks
                .Select(o => o.Text).ToList();
        }

        private void SaveIndices(IList<Token> tokens, IList<string> linkTexts, UrlFile urlFile, ulong oldUrlFileId)
        {
            var invertedIndices = new List<Index>();
            var postingLists = new List<PostingList>();

            foreach (var token in tokens)
            {
                var key = new IndexKey
                {
                    Word = token.Word,
                    UrlFileId = urlFile.UrlFileId,
                };

                var weight = ScoringHelper.CalculateIndexWeight(urlFile.Title,
                                                            urlFile.TextContent,
                                                            urlFile.Url,
                                                            DateTime.FromBinary((long)urlFile.PublishDate),
                                                            token.OccurencesInTitle,
                                                            token.OccurencesInLinks,
                                                            linkTexts,
                                                            token.Word,
                                                            token.WordFrequency,
                                                            token.Positions);
                var index = new Index
                {
                    Key = key,
                    WordFrequency = token.WordFrequency,
                    OccurencesInTitle = token.OccurencesInTitle,
                    OccurencesInLinks = token.OccurencesInLinks,
                    OccurencesInHeaders = token.OccurencesInHeaders,
                    Weight = weight,
                };
                index.Positions.AddRange(token.Positions);

                invertedIndices.Add(index);

                var postingList = new PostingList
                {
                    Word = token.Word,
                    WordFrequency = token.WordFrequency,
                    DocumentFrequency = 1,
                    IsAdd = true,
                };
                var posting = new Posting
                {
                    UrlFileId = urlFile.UrlFileId,
                    Weight = weight,
                };
                postingList.Postings.Add(posting);

                postingLists.Add(postingList);
            }
            mConfig.PostingListStore.SavePostingLists(urlFile.UrlFileId, postingLists);
            mConfig.InvertedIndexStore.SaveIndices(urlFile.UrlFileId, invertedIndices);
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
                    fetchedFile = FetchUrl(url);
                    if (fetchedFile == null)
                    {
                        return;
                    }

                    // Get InLink Texts
                    var inLinkTexts = GetInLinkTexts(fetchedFile.Url);

                    // Parse File
                    var (urlFile, linkList, tokens) = ParseFetchedFile(fetchedFile, inLinkTexts);


                    if (GetSameUrl(fetchedFile, urlFile.Content) != null)
                    {
                        // Has Same UrlFile, Skip
                        return;
                    }

                    // Save New UrlFile
                    // Get Old id and New id
                    ulong oldUrlFileId;
                    (urlFile, oldUrlFileId) = mConfig.UrlFileStore.SaveUrlFileAndGetOldId(urlFile);

                    // Clear old data
                    if (oldUrlFileId != 0)
                    {
                        mConfig.PostingListStore.ClearPostingLists(oldUrlFileId);
                        mConfig.InvertedIndexStore.ClearIndices(oldUrlFileId);
                        mConfig.LinkStore.ClearLinks(oldUrlFileId);
                    }

                    lock (mContentUrlDictSyncLock)
                    {
                        mContentUrlDict.Remove(urlFile.Content);
                    }

                    // Filter Links
                    linkList = FilterLinks(linkList);

                    // Save links
                    mConfig.LinkStore.SaveLinks(urlFile.UrlFileId,
                        linkList.Select(o => new Link
                        {
                            Text = o.Text,
                            Url = o.Url,
                            UrlFileId = urlFile.UrlFileId,
                        }));

                    // Save Indices
                    SaveIndices(tokens, inLinkTexts, urlFile, oldUrlFileId);

                    // Add newly-found urls
                    var urls = linkList.Select(o => o.Url).Distinct();
                    mUrlFrontier.PushUrls(urls);

                    // Push Back This Url
                    mUrlFrontier.PushBackUrl(url, urlFile.UpdateInterval);

                    mLogger.Log(nameof(Crawler), "End Crawl: " + url);
                }
                catch (NotSupportedException e)
                {
                    mLogger.LogException(nameof(Crawler), "Not supported file format: " + url, e, false);
                    mErrorLogger.LogException(nameof(Crawler), "Not supported file format: " + url, e);

                    // Retry
                    mUrlFrontier.PushBackUrl(url, 0, true);
                }
                catch (InvalidDataException e)
                {
                    mLogger.LogException(nameof(Crawler), "Invalid data: " + url, e);
                    mErrorLogger.LogException(nameof(Crawler), "Invalid data: " + url, e);

                    // Retry
                    mUrlFrontier.PushBackUrl(url, 0, true);
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
                    mUrlFrontier.PushBackUrl(url, 0);
                }
                catch (Exception e)
                {
                    mLogger.LogException(nameof(Crawler), "Failed to crawl url: " + url, e);
                    mErrorLogger.LogException(nameof(Crawler), "Failed to crawl url: " + url, e);

                    // Retry
                    mUrlFrontier.PushBackUrl(url, 0, true);
                }
                finally
                {
                    mFetchSemaphore.Release();
                    if (fetchedFile != null && File.Exists(fetchedFile.FilePath))
                    {
                        File.Delete(fetchedFile.FilePath);
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
