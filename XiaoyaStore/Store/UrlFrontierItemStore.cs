using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Service;

namespace XiaoyaStore.Store
{
    public class UrlFrontierItemStore : IDisposable, IUrlFrontierItemStore
    {
        readonly Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        readonly UrlFrontierItemService.UrlFrontierItemServiceClient client;
        public UrlFrontierItemStore()
        {
            client = new UrlFrontierItemService.UrlFrontierItemServiceClient(channel);
        }

        public bool Init(IEnumerable<string> urls)
        {
            var arg = new ArgUrls();
            arg.Urls.AddRange(urls);

            var result = client.Init(arg);
            return result.IsSuccessful;
        }

        public bool PushUrls(IEnumerable<string> urls)
        {
            var arg = new ArgUrls();
            arg.Urls.AddRange(urls);

            var result = client.PushUrls(arg);
            return result.IsSuccessful;
        }

        public bool PushBackUrl(string url, ulong updateInterval, bool failed = false)
        {
            var arg = new ArgPushBackUrl
            {
                Url = url,
                UpdateInterval = updateInterval,
                Failed = failed
            };
            var result = client.PushBackUrl(arg);
            return result.IsSuccessful;
        }

        public string PopUrl()
        {
            var result = client.PopUrl(new ArgVoid());
            if (result.IsSuccessful)
            {
                return result.Url;
            }
            else
            {
                return null;
            }
        }

        public bool RemoveUrl(string url)
        {
            var arg = new ArgUrl
            {
                Url = url
            };
            var result = client.RemoveUrl(arg);
            return result.IsSuccessful;
        }

        public ulong GetHostCount(string host)
        {
            var arg = new ArgHost
            {
                Host = host
            };
            var result = client.GetHostCount(arg);
            if (result.IsSuccessful)
            {
                return result.Count;
            }
            else
            {
                return 0;
            }
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        ~UrlFrontierItemStore()
        {
            if (channel.State != ChannelState.Shutdown)
            {
                Dispose();
            }
        }
    }
}
