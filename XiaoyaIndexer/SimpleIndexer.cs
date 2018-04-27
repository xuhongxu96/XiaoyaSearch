using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaFileParser;
using XiaoyaFileParser.Model;
using XiaoyaStore.Store;
using XiaoyaStore.Data.Model;
using XiaoyaIndexer.Config;
using XiaoyaLogger;
using System.IO;
using static XiaoyaStore.Data.Model.InvertedIndex;
using static XiaoyaFileParser.Model.Token;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using XiaoyaNLP.Encoding;
using XiaoyaCommon.Helper;

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

            mConfig.UrlFileStore.RestartIndex();
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

                    UniversalFileParser parser = new UniversalFileParser
                    {
                        UrlFile = urlFile
                    };

                    var links = mConfig.LinkStore.LoadByUrl(urlFile.Url);
                    var linkTexts = links.Select(o => o.Text).ToList();

                    IList<Token> tokens = parser.GetTokensAsync(linkTexts).GetAwaiter().GetResult();

                    var invertedIndices = (from token in tokens
                                           select new InvertedIndex
                                           {
                                               Word = token.Word,
                                               Positions = string.Join(",", token.Positions),
                                               UrlFileId = urlFile.UrlFileId,
                                               WordFrequency = token.WordFrequency,
                                               OccurencesInTitle = token.OccurenceInTitle,
                                               OccurencesInLinks = token.OccurenceInLinks,
                                               Weight = ScoringHelper.CalculateIndexWeight(urlFile.Title,
                                                                                           urlFile.Content,
                                                                                           urlFile.Url,
                                                                                           urlFile.PublishDate,
                                                                                           token.OccurenceInTitle,
                                                                                           token.OccurenceInLinks,
                                                                                           linkTexts,
                                                                                           token.Word,
                                                                                           token.WordFrequency,
                                                                                           /*
                                                                                           mConfig.IndexStatStore.LoadByWord(token.Word)?.WordFrequency ?? 0,
                                                                                           mConfig.IndexStatStore.LoadByWord(token.Word)?.DocumentFrequency ?? 0,
                                                                                           mConfig.UrlFileStore.Count(),
                                                                                           */
                                                                                           token.Positions),
                                           }).ToList();

                    mConfig.InvertedIndexStore.ClearAndSaveInvertedIndices(urlFile, invertedIndices);

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

                var urlFile = mConfig.UrlFileStore.LoadAnyForIndex();
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
