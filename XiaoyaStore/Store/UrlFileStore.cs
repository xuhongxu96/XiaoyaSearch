using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Model;
using XiaoyaStore.Service;

namespace XiaoyaStore.Store
{
    public class UrlFileStore : IUrlFileStore
    {
        readonly Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        readonly UrlFileService.UrlFileServiceClient client;
        public UrlFileStore()
        {
            client = new UrlFileService.UrlFileServiceClient(channel);
        }

        public UrlFile GetUrlFile(ulong id)
        {
            var arg = new ArgId
            {
                Id = id
            };
            var result = client.GetUrlFileById(arg);
            if (result.IsSuccessful)
            {
                return result.Urlfile;
            }
            else
            {
                return null;
            }
        }

        public UrlFile GetUrlFile(string url)
        {
            var arg = new ArgUrl
            {
                Url = url
            };
            var result = client.GetUrlFileByUrl(arg);
            if (result.IsSuccessful)
            {
                return result.Urlfile;
            }
            else
            {
                return null;
            }
        }

        public IList<UrlFile> GetUrlFilesByHash(string hash)
        {
            var arg = new ArgHash
            {
                Hash = hash
            };
            var result = client.GetUrlFilesByHash(arg);
            if (result.IsSuccessful)
            {
                return result.Urlfiles;
            }
            else
            {
                return new List<UrlFile>();
            }
        }

        public (UrlFile urlFile, ulong oldUrlFileId) SaveUrlFileAndGetOldId(UrlFile urlFile)
        {
            var arg = new ArgUrlFile
            {
                Urlfile = urlFile
            };
            var result = client.SaveUrlFileAndGetOldId(arg);
            if (result.IsSuccessful)
            {
                return (result.Urlfile, result.OldUrlfileId);
            }
            else
            {
                return (null, 0);
            }
        }

        public ulong GetCount()
        {
            var result = client.GetCount(new ArgVoid());
            if (result.IsSuccessful)
            {
                return result.Count;
            }
            else
            {
                return 0;
            }
        }

        public UrlFile GetForIndex()
        {
            var result = client.GetForIndex(new ArgVoid());
            if (result.IsSuccessful)
            {
                return result.Urlfile;
            }
            else
            {
                return null;
            }
        }

        public bool FinishIndex(string url)
        {
            var arg = new ArgUrl
            {
                Url = url
            };
            var result = client.FinishIndex(arg);
            return result.IsSuccessful;
        }

        public void Dispose()
        {
            channel.ShutdownAsync().Wait();
        }

        ~UrlFileStore()
        {
            if (channel.State != ChannelState.Shutdown)
            {
                Dispose();
            }
        }
    }
}
