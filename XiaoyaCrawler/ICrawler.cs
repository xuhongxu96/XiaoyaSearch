using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XiaoyaCrawler
{
    public enum CrawlerStatus
    {
        STOPPED, RUNNING
    }

    public interface ICrawler
    {
        Task StartAsync(bool restart = false);
        Task StopAsync();
        CrawlerStatus Status { get; }
    }
}
