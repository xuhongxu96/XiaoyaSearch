using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Model;
using XiaoyaStore.Service;

namespace XiaoyaStore.Store
{
    public class PostingListStore : IPostingListStore
    {
        readonly Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        readonly PostingListService.PostingListServiceClient client;
        public PostingListStore()
        {
            client = new PostingListService.PostingListServiceClient(channel);
        }

        public bool SavePostingLists(ulong urlFileId, IEnumerable<PostingList> postingLists)
        {
            var arg = new ArgSavePostingLists
            {
                UrlFileId = urlFileId
            };
            arg.PostingList.AddRange(postingLists);
            var result = client.SavePostingLists(arg);
            return result.IsSuccessful;
        }

        public bool ClearPostingLists(ulong urlFileId)
        {
            var arg = new ArgId
            {
                Id = urlFileId
            };
            var result = client.ClearPostingLists(arg);
            return result.IsSuccessful;
        }

        public PostingList GetPostingList(string word)
        {
            var arg = new ArgWord
            {
                Word = word
            };
            var result = client.GetPostingList(arg);
            if (result.IsSuccessful)
            {
                return result.Postinglist;
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        ~PostingListStore()
        {
            if (channel.State != ChannelState.Shutdown)
            {
                Dispose();
            }
        }
    }
}
