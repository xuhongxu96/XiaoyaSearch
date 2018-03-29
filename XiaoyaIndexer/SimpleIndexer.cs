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

        public bool IsWaiting { get; private set; } = false;

        public SimpleIndexer(IndexerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Indexer.Log"));
            mConfig = config;

            mConfig.UrlFileStore.RestartIndex();
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

        protected async Task IndexFile(UrlFile urlFile)
        {
            try
            {
                mLogger.Log(nameof(SimpleIndexer), "Indexing Url: " + urlFile.Url);
                UniversalFileParser parser = new UniversalFileParser
                {
                    UrlFile = urlFile
                };
                IList<Token> tokens = await parser.GetTokensAsync();

                var invertedIndices = from token in tokens
                                      select new InvertedIndex
                                      {
                                          Word = token.Text,
                                          Position = token.Position,
                                          UrlFileId = urlFile.UrlFileId,
                                          IndexType = ConvertType(token.Type),
                                      };

                lock (mSyncLock)
                {
                    mConfig.InvertedIndexStore.ClearAndSaveInvertedIndices(urlFile, invertedIndices);
                }

                mLogger.Log(nameof(SimpleIndexer), "Indexed Url: " + urlFile.Url);
            }
            finally
            {
                mIndexSemaphore.Release();
            }
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

                mIndexSemaphore.Wait();
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

                mTasks.Add(IndexFile(urlFile));
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
