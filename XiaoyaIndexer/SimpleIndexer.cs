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

namespace XiaoyaIndexer
{
    public class SimpleIndexer : IIndexer
    {
        protected IndexerConfig mConfig;
        protected RuntimeLogger mLogger;
        protected RuntimeLogger mErrorLogger;
        protected CancellationTokenSource mCancellationTokenSource = null;

        private SemaphoreSlim mIndexSemaphore;
        private ConcurrentBag<Task> mTasks = new ConcurrentBag<Task>();
        private object mSyncLock = new object();

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
            mIndexSemaphore.Wait();

            mTasks.Add(Task.Run(() =>
            {
                try
                {
                    mLogger.Log(nameof(SimpleIndexer), "Indexing Url: " + urlFile.Url);

                    if (!EncodingDetector.IsValidString(urlFile.Content))
                    {
                        throw new NotSupportedException("Invalid content");
                    }

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

                    // lock (mSyncLock)
                    {
                        mConfig.InvertedIndexStore.ClearAndSaveInvertedIndices(urlFile, invertedIndices);
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
            }));
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
