using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaFileParser;
using XiaoyaFileParser.Model;
using XiaoyaStore.Store;
using XiaoyaIndexer.Config;
using XiaoyaLogger;
using System.IO;
using static XiaoyaFileParser.Model.Token;
using System.Collections.Concurrent;
using XiaoyaNLP.Encoding;
using XiaoyaCommon.Helper;
using XiaoyaStore.Model;

namespace XiaoyaIndexer
{
    public class SimpleIndexer : IIndexer
    {
        protected IndexerConfig mConfig;
        protected RuntimeLogger mLogger;
        protected RuntimeLogger mErrorLogger;
        protected CancellationTokenSource mCancellationTokenSource = null;

        private SemaphoreSlim mIndexSemaphore;
        private ConcurrentDictionary<Task, bool> mTasks = new ConcurrentDictionary<Task, bool>();

        public bool IsWaiting { get; private set; } = false;

        public SimpleIndexer(IndexerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Indexer.Log"), true);
            mErrorLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Indexer Error.Log"), true);
            mConfig = config;
        }

        public void WaitAll()
        {
            Task.WaitAll(mTasks.Keys.ToArray());
        }

        protected void IndexFile(UrlFile urlFile)
        {
            mIndexSemaphore.Wait();
            mLogger.Log(nameof(SimpleIndexer), "Indexing Url: " + urlFile.Url);
            mTasks.TryAdd(Task.Run(() =>
            {
                try
                {
                    UniversalFileParser parser = new UniversalFileParser();
                    parser.SetFile(urlFile.MimeType, urlFile.Url, urlFile.Charset, null,
                        urlFile.Content, urlFile.TextContent, urlFile.Title);

#if DEBUG
                    var time = DateTime.Now;
                    Console.WriteLine("Loading Links: " + urlFile.Url);
#endif

                    var links = mConfig.LinkStore.GetLinksByUrl(urlFile.Url);
                    var linkTexts = links.Select(o => o.Text).ToList();

                    IList<Token> tokens = parser.GetTokensAsync(linkTexts).GetAwaiter().GetResult();
#if DEBUG
                    Console.WriteLine("Loaded Links: " + urlFile.Url + "\n" + (DateTime.Now - time).TotalSeconds);
                    time = DateTime.Now;
#endif
                    var headers = parser.GetHeadersAsync().GetAwaiter().GetResult();
                    urlFile.HeaderTotalLength = (uint) headers.Sum(o => o.Text.Length);
                    urlFile.HeaderCount = (uint) headers.Sum(o => (6 - o.Level));

                    urlFile.InLinkCount = (uint) linkTexts.Count;
                    urlFile.InLinkTotalLength = (uint) linkTexts.Sum(o => o.Length);

                    ulong oldUrlFileId;
                    (urlFile, oldUrlFileId) = mConfig.UrlFileStore.SaveUrlFileAndGetOldId(urlFile);

#if DEBUG
                    Console.WriteLine("Saved Header/Link Properties: " + urlFile.Url + "\n" + (DateTime.Now - time).TotalSeconds);
                    time = DateTime.Now;
#endif

                    var invertedIndices = new List<Index>();
                    foreach (var token in tokens)
                    {
                        var key = new IndexKey
                        {
                            Word = token.Word,
                            UrlfileId = urlFile.UrlfileId,
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
                        postingList.Postings.Add(urlFile.UrlfileId);
                        mConfig.PostingListStore.SavePostingList(postingList);
                    }

#if DEBUG
                    Console.WriteLine("Generated Inverted Indices: " + urlFile.Url + "\n" + (DateTime.Now - time).TotalSeconds);
                    time = DateTime.Now;
#endif
                    mConfig.InvertedIndexStore.ClearAndSaveIndicesOf(urlFile.UrlfileId, oldUrlFileId, invertedIndices);

#if DEBUG
                    Console.WriteLine("Saved Indices: " + urlFile.Url + "\n" + (DateTime.Now - time).TotalSeconds);
                    time = DateTime.Now;
#endif

                    if (File.Exists(urlFile.FilePath))
                    {
                        File.Delete(urlFile.FilePath);
                    }

                    mLogger.Log(nameof(SimpleIndexer), "Indexed Url: " + urlFile.Url);
                }
                catch (Exception e)
                {
                    mLogger.LogException(nameof(SimpleIndexer), "Failed to index url: " + urlFile.Url, e);
                    mErrorLogger.LogException(nameof(SimpleIndexer), "Failed to index url: " + urlFile.Url, e);
                }
                finally
                {
                    mIndexSemaphore.Release();
                }
            }).ContinueWith(task =>
            {
                mTasks.TryRemove(task, out bool value);
            }), true);
        }

        public async Task CreateIndexAsync()
        {
            mCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = mCancellationTokenSource.Token;

            mIndexSemaphore = new SemaphoreSlim(mConfig.MaxIndexingConcurrency);

            int waitSeconds = 1;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var urlFile = mConfig.UrlFileStore.GetForIndex();
                if (urlFile == null)
                {
                    IsWaiting = true;
                    await Task.Run(() => Thread.Sleep(waitSeconds * 1000));
                    if (waitSeconds < 32)
                    {
                        waitSeconds *= 2;
                    }
                    continue;
                }
                IsWaiting = false;
                waitSeconds = 1;

                IndexFile(urlFile);
            }
        }

        public void StopIndex()
        {
            if (mCancellationTokenSource != null)
            {
                mCancellationTokenSource.Cancel();
            }
        }
    }
}
