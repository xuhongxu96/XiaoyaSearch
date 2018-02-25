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

namespace XiaoyaIndexer
{
    public class SimpleIndexer : IIndexer
    {

        protected CancellationTokenSource mCancellationTokenSource = null;
        protected UniversalFileParser mParser = new UniversalFileParser();
        protected IndexerConfig mConfig = new IndexerConfig();

        public SimpleIndexer() { }

        public SimpleIndexer(IndexerConfig config)
        {
            mConfig = config;
        }

        protected async Task IndexFilesAsync(CancellationToken cancellationToken)
        {
            while (true)
            {

                cancellationToken.ThrowIfCancellationRequested();

                var urlFile = await mConfig.UrlFileStore.LoadAnyForIndexAsync();
                if (urlFile == null)
                {
                    await Task.Run(() => Thread.Sleep(5000));
                    continue;
                }

                mParser.UrlFile = urlFile;
                IList<Token> tokens = await mParser.GetTokensAsync();

                var invertedIndices = from token in tokens
                                      select new InvertedIndex
                                      {
                                          Word = token.Text,
                                          Position = token.Position,
                                          UrlFileId = urlFile.UrlFileId,
                                      };

                await mConfig.InvertedIndexStore.ClearInvertedIndicesFor(urlFile);
                await mConfig.InvertedIndexStore.SaveInvertedIndicesAsync(invertedIndices);
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
