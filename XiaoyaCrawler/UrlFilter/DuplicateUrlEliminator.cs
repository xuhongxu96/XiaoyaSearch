using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaLogger;

namespace XiaoyaCrawler.UrlFilter
{
    public class DuplicateUrlEliminator : IUrlFilter
    {

        protected CrawlerConfig mConfig;
        protected ConcurrentDictionary<string, bool> mUrlSet;
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


        public DuplicateUrlEliminator(CrawlerConfig config)
        {
            mConfig = config;
            mUrlSet = new ConcurrentDictionary<string, bool>();
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "DuplicateUrlEliminator.log"));
            mCheckPointFileName = Path.Combine(config.CheckPointDirectory, "DuplicateUrlEliminatorCheckPoint.ckp");
            if (!Directory.Exists(Path.GetDirectoryName(mCheckPointFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(mCheckPointFileName));
            }
        }

        public IList<string> Filter(IList<string> urls)
        {
            var result = urls.Except(mUrlSet.Keys).ToList();
            foreach (var url in result)
            {
                mUrlSet.TryAdd(url, true);
            }
            return result;
        }

        public async Task LoadCheckPoint()
        {
            if (!File.Exists(mCheckPointFileName))
            {
                return;
            }
            mLogger.Log(nameof(DuplicateUrlEliminator), "Load Check Point: Begin");

            await Task.Run(() =>
            {
                lock (mSyncLock)
                {
                    mUrlSet.Clear();
                    using (var reader = new StreamReader(mCheckPointFileName))
                    {
                        while (!reader.EndOfStream)
                        {
                            mUrlSet.TryAdd(reader.ReadLine(), true);
                        }
                    }
                }
            });

            mLogger.Log(nameof(DuplicateUrlEliminator), "Load Check Point: End");
        }

        public async Task SaveCheckPoint()
        {
            // If last checkpoint hasn't been saved yet, skip this checkpoint task
            if (mSaveCheckPointTask != null && !mSaveCheckPointTask.IsCompleted)
            {
                return;
            }

            mLogger.Log(nameof(DuplicateUrlEliminator), "Save Check Point: Begin");

            mSaveCheckPointTask = Task.Run(() =>
            {
                lock (mSyncLock)
                {
                    using (var writer = new StreamWriter(mCheckPointFileName))
                    {
                        foreach (var url in mUrlSet)
                        {
                            writer.WriteLine(url.Key);
                        }
                        writer.Flush();
                    }
                }
            });
            await mSaveCheckPointTask;

            mLogger.Log(nameof(DuplicateUrlEliminator), "Save Check Point: End");
        }
    }
}
