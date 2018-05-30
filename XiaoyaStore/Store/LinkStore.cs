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

        public bool RemoveLinks(IEnumerable<Link> links)
        {
            var arg = new ArgLinks();
            arg.Links.AddRange(links);
            var result = client.RemoveLinks(arg);
            return result.IsSuccessful;
        }

        public bool SaveLinks(IEnumerable<Link> links)
        {
            var arg = new ArgLinks();
            arg.Links.AddRange(links);
            var result = client.SaveLinks(arg);
            return result.IsSuccessful;
        }
        
        public IList<Link> GetLinks(string url)
        {
            var arg = new ArgUrl
            {
                Url = url
            };
            var result = client.GetLinks(arg);
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
