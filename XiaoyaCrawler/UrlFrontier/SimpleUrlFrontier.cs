using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaLogger;

namespace XiaoyaCrawler.UrlFrontier
{
    public class SimpleUrlFrontier : IUrlFrontier
    {
        /// <summary>
        /// Url queue
        /// </summary>
        protected ConcurrentQueue<string> mUrlQueue;
        /// <summary>
        /// Check point file name
        /// </summary>
        protected string mCheckPointFileName;
        /// <summary>
        /// Logger
        /// </summary>
        protected RuntimeLogger mLogger;
        
        /// <summary>
        /// Task to save check point
        /// </summary>
        private Task mSaveCheckPointTask = null;
        /// <summary>
        /// Sync lock
        /// </summary>
        private object mSyncLock = new object();

        /// <summary>
        /// Is url queue empty
        /// </summary>
        public bool IsEmpty => mUrlQueue.Count == 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="checkPointFileName">Check point file name</param>
        public SimpleUrlFrontier(CrawlerConfig config)
        {
            mUrlQueue = new ConcurrentQueue<string>(config.InitUrls);
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "SimpleUrlFrontier.log"));

            mCheckPointFileName = Path.Combine(config.CheckPointDirectory, "SimpleUrlFrontierCheckPoint.ckp");
            if (!Directory.Exists(Path.GetDirectoryName(mCheckPointFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(mCheckPointFileName));
            }
        }

        public async Task LoadCheckPoint()
        {
            if (!File.Exists(mCheckPointFileName))
            {
                return;
            }

            mLogger.Log(nameof(SimpleUrlFrontier), "Load Check Point: Begin");

            await Task.Run(() =>
            {
                lock (mSyncLock)
                {
                    mUrlQueue.Clear();
                    using (var reader = new StreamReader(mCheckPointFileName))
                    {
                        while (!reader.EndOfStream)
                        {
                            mUrlQueue.Enqueue(reader.ReadLine());
                        }
                    }
                }
            });

            mLogger.Log(nameof(SimpleUrlFrontier), "Load Check Point: End");
        }

        /// <summary>
        /// Save check point
        /// </summary>
        public async Task SaveCheckPoint()
        {
            // If last checkpoint hasn't been saved yet, skip this checkpoint task
            if (mSaveCheckPointTask != null && !mSaveCheckPointTask.IsCompleted)
            {
                return;
            }

            mLogger.Log(nameof(SimpleUrlFrontier), "Save Check Point: Begin");

            mSaveCheckPointTask = Task.Run(() =>
            {
                lock (mSyncLock)
                {
                    using (var writer = new StreamWriter(mCheckPointFileName))
                    {
                        foreach (var url in mUrlQueue)
                        {
                            writer.WriteLine(url);
                        }
                        writer.Flush();
                    }
                }
            });
            await mSaveCheckPointTask;

            mLogger.Log(nameof(SimpleUrlFrontier), "Save Check Point: End");
        }

        /// <summary>
        /// Pop next url to be fetched
        /// </summary>
        /// <returns>Url to be fetched</returns>
        public string PopUrl()
        {
            if (!mUrlQueue.TryDequeue(out string url))
            {
                mLogger.Log(nameof(SimpleUrlFrontier), "Failed to Pop Url");
                return null;
            }

            mLogger.Log(nameof(SimpleUrlFrontier), "Pop Url: " + url);

            return url;
        }

        /// <summary>
        /// Push a new url
        /// </summary>
        /// <param name="url">New url</param>
        public void PushUrl(string url)
        {
            mLogger.Log(nameof(SimpleUrlFrontier), "Push Url: " + url);

            mUrlQueue.Enqueue(url);
        }
    }
}
