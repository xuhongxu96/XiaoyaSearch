using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Model;
using XiaoyaStore.Service;

namespace XiaoyaStore.Store
{
    public class LinkStore : ILinkStore
    {
        readonly Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        readonly LinkService.LinkServiceClient client;
        public LinkStore()
        {
            client = new LinkService.LinkServiceClient(channel);
        }

        public bool SaveLinksOfUrlFile(ulong urlFileId, ulong oldUrlFileId, IEnumerable<Link> links)
        {
            var arg = new ArgSaveLinkOfUrlFile
            {
                UrlfileId = urlFileId,
                OldUrlfileId = oldUrlFileId,
            };
            arg.Links.AddRange(links);
            var result = client.SaveLinksOfUrlFile(arg);
            return result.IsSuccessful;
        }
        
        public Link GetLink(ulong id)
        {
            var arg = new ArgId
            {
                Id = id
            };
            var result = client.GetLinkById(arg);
            if (result.IsSuccessful)
            {
                return result.Link;
            }
            else
            {
                return null;
            }
        }

        public IList<Link> GetLinksByUrl(string url)
        {
            var arg = new ArgUrl
            {
                Url = url
            };
            var result = client.GetLinksByUrl(arg);
            if (result.IsSuccessful)
            {
                return result.Links;
            }
            else
            {
                return new List<Link>();
            }
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        ~LinkStore()
        {
            if (channel.State != ChannelState.Shutdown)
            {
                Dispose();
            }
        }
    }
}
