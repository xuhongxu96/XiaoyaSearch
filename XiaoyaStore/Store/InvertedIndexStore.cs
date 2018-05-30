using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Model;
using XiaoyaStore.Service;

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : IInvertedIndexStore
    {
        readonly Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        readonly InvertedIndexService.InvertedIndexServiceClient client;
        public InvertedIndexStore()
        {
            client = new InvertedIndexService.InvertedIndexServiceClient(channel);
        }

        public bool SaveIndices(ulong urlFileId, IEnumerable<Index> indices)
        {
            var arg = new ArgSaveIndices
            {
                UrlFileId = urlFileId,
            };
            arg.Indices.AddRange(indices);
            var result = client.SaveIndices(arg);
            return result.IsSuccessful;
        }

        public Index GetIndex(ulong urlFileId, string word)
        {
            var arg = new ArgIndexKey
            {
                UrlFileId = urlFileId,
                Word = word,
            };
            var result = client.GetIndex(arg);
            if (result.IsSuccessful)
            {
                return result.Index;
            }
            else
            {
                return null;
            }
        }

        public bool ClearIndices(ulong urlFileId)
        {
            var arg = new ArgId
            {
                Id = urlFileId
            };
            var result = client.ClearIndices(arg);
            return result.IsSuccessful;
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        ~InvertedIndexStore()
        {
            if (channel.State != ChannelState.Shutdown)
            {
                Dispose();
            }
        }
    }
}
