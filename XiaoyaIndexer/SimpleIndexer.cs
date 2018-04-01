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

namespace XiaoyaIndexer
{
    public class SimpleIndexer : IIndexer
    {
        protected CancellationTokenSource mCancellationTokenSource = null;
        protected IndexerConfig mConfig;
        protected RuntimeLogger mLogger;
        protected SemaphoreSlim mIndexSemaphore;
        protected ConcurrentBag<Task> mTasks = new ConcurrentBag<Task>();
        protected object mSyncLock = new object();
        protected Timer mTimer;

        public bool IsWaiting { get; private set; } = false;

        public SimpleIndexer(IndexerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Indexer.Log"), true);
            mConfig = config;

            mConfig.UrlFileStore.RestartIndex();
            mTimer = new Timer(new TimerCallback(obj =>
            {
                mLogger.Log(nameof(SimpleIndexer), "Generating Index Stats...");

                lock (mSyncLock)
                {
                    mConfig.InvertedIndexStore.GenerateStat();
                }

                mLogger.Log(nameof(SimpleIndexer), "Generated Index Stats.");
            }), null, TimeSpan.FromTicks(0), TimeSpan.FromMinutes(10));
        }

        public void WaitAll()
        {
            Task.WaitAll(mTasks.ToArray());
        }

        protected static InvertedIndexType ConvertType(TokenType type)
        {
            switch (type)
            {
                case TokenType.Body:
                default:
                    return InvertedIndexType.Body;
                case TokenType.Title:
                    return InvertedIndexType.Title;
            }
        }

        protected void IndexFile(UrlFile urlFile)
        {
            mTasks.Add(Task.Run(() =>
            {
                try
                {
                    mLogger.Log(nameof(SimpleIndexer), "Indexing Url: " + urlFile.Url);
                    UniversalFileParser parser = new UniversalFileParser
                    {
                        UrlFile = urlFile
                    };
                    IList<Token> tokens = parser.GetTokensAsync().GetAwaiter().GetResult();

                    var invertedIndices = from token in tokens
                                          select new InvertedIndex
                                          {
                                              Word = token.Text,
                                              Position = token.Position,
                                              UrlFileId = urlFile.UrlFileId,
                                              IndexType = ConvertType(token.Type),
                                          };
                    int failedTimes = 0;
                    while (failedTimes != -1)
                    {
                        try
                        {
                            mConfig.InvertedIndexStore.ClearAndSaveInvertedIndices(urlFile, invertedIndices);
                            failedTimes = -1;
                        }
                        catch (DbUpdateException)
                        {
                            Thread.Sleep(5000);
                            failedTimes++;
                            if (failedTimes == 10)
                            {
                                mLogger.Log(nameof(SimpleIndexer), "Failed to Index Url: " + urlFile.Url);
                                throw;
                            }
                        }
                    }

                    mLogger.Log(nameof(SimpleIndexer), "Indexed Url: " + urlFile.Url);
                }
                catch (Exception e)
                {
                    mLogger.Log(nameof(SimpleIndexer), "Failed to Index Url: " + urlFile.Url + "\r\n" + e.Message + "\r\n" + e.InnerException.Message);
                }
                finally
                {
                    mIndexSemaphore.Release();
                }
            }));
        }

        protected async Task IndexFilesAsync(CancellationToken cancellationToken)
        {
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

                mIndexSemaphore.Wait();
                IndexFile(urlFile);
            }
        }

        public async Task CreateIndexAsync()
        {
            mCancellationTokenSource = new CancellationTokenSource();
            await IndexFilesAsync(mCancellationTokenSource.Token);
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
