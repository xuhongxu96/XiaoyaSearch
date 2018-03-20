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

namespace XiaoyaIndexer
{
    public class SimpleIndexer : IIndexer
    {

        protected CancellationTokenSource mCancellationTokenSource = null;
        protected UniversalFileParser mParser = new UniversalFileParser();
        protected IndexerConfig mConfig;
        protected RuntimeLogger mLogger;

        public SimpleIndexer(IndexerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Indexer.Log"));
            mConfig = config;
        }

        protected async Task IndexFilesAsync(CancellationToken cancellationToken)
        {
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
                    await Task.Run(() => Thread.Sleep(waitSeconds * 1000));
                    if (waitSeconds < 32)
                    {
                        waitSeconds *= 2;
                    }
                    continue;
                }
                waitSeconds = 1;

                mLogger.Log(nameof(SimpleIndexer), "Indexing Url: " + urlFile.Url);

                mParser.UrlFile = urlFile;
                IList<Token> tokens = await mParser.GetTokensAsync();

                var invertedIndices = from token in tokens
                                      select new InvertedIndex
                                      {
                                          Word = token.Text,
                                          Position = token.Position,
                                          UrlFileId = urlFile.UrlFileId,
                                      };

                mConfig.InvertedIndexStore.ClearInvertedIndicesOf(urlFile);
                mConfig.InvertedIndexStore.SaveInvertedIndices(invertedIndices);

                mLogger.Log(nameof(SimpleIndexer), "Indexed Url: " + urlFile.Url);
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
