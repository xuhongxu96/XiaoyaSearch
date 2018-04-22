using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using Microsoft.EntityFrameworkCore;
using static XiaoyaStore.Data.Model.InvertedIndex;
using System.Data.SqlClient;
using XiaoyaStore.Helper;
using XiaoyaStore.Cache;
using System.Collections.Concurrent;
using EFCore.BulkExtensions;
using XiaoyaLogger;
using System.Timers;
using System.Threading;

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : BaseStore, IInvertedIndexStore
    {
        protected object mSyncLock = new object();
        protected object mDictSyncLock = new object();

        public struct CacheIndex
        {
            public int urlFileId;
            public string word;

            public override bool Equals(object obj)
            {
                if (!(obj is CacheIndex))
                {
                    return false;
                }

                var index = (CacheIndex)obj;
                return urlFileId == index.urlFileId &&
                       word == index.word;
            }

            public override int GetHashCode()
            {
                var hashCode = -1340907702;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + urlFileId.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(word);
                return hashCode;
            }
        }

        public struct IndexStatDeltaItem
        {
            public string word;
            public long wordFrequencyDelta;
            public int documentFrequencyDelta;
        }

        protected LRUCache<CacheIndex, InvertedIndex> mCache;
        protected ConcurrentDictionary<string, IndexStatDeltaItem> mIndexStatDict = new ConcurrentDictionary<string, IndexStatDeltaItem>();

        public InvertedIndexStore(DbContextOptions options = null, bool enableCache = true, RuntimeLogger logger = null) : base(options)
        {
            mCache = new LRUCache<CacheIndex, InvertedIndex>(
                TimeSpan.FromDays(5), GetCache, LoadCaches, 30_000_000, enableCache);

            var thread = new Thread(FlushIndexStat)
            {
                Priority = ThreadPriority.AboveNormal,
            };
            thread.Start();
        }

        public void CacheWordsInUrlFiles(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            mCache.LoadCaches(() => LoadCachesOfWordsInUrlFiles(urlFileIds, words));
        }

        protected IEnumerable<Tuple<CacheIndex, InvertedIndex>> LoadCachesOfWordsInUrlFiles(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            using (var context = NewContext())
            {
                foreach (var item in context.InvertedIndices
                    .Where(o => urlFileIds.Contains(o.UrlFileId) && words.Contains(o.Word))
                    .OrderByDescending(o => o.Weight))
                {
                    yield return Tuple.Create(new CacheIndex
                    {
                        urlFileId = item.UrlFileId,
                        word = item.Word,
                    }, item);
                }
            }
        }

        protected IEnumerable<Tuple<CacheIndex, InvertedIndex>> LoadCaches()
        {
            using (var context = NewContext())
            {
                foreach (var item in context.InvertedIndices.OrderByDescending(o => o.Weight))
                {
                    yield return Tuple.Create(new CacheIndex
                    {
                        urlFileId = item.UrlFileId,
                        word = item.Word,
                    }, item);
                }
            }
        }

        protected InvertedIndex GetCache(CacheIndex cacheIndex)
        {
            using (var context = NewContext())
            {
                Console.WriteLine(cacheIndex.word + " " + cacheIndex.urlFileId);
                return context.InvertedIndices
                    .AsNoTracking()
                    .SingleOrDefault(o => o.UrlFileId == cacheIndex.urlFileId
                                     && o.Word == cacheIndex.word);
            }
        }

        public void ClearInvertedIndicesOf(int urlFileId)
        {
            using (var context = NewContext())
            {
                context.BulkDelete(context.InvertedIndices.Where(o => o.UrlFileId == urlFileId).ToList());
            }
        }

        private void SaveIndexStats(XiaoyaSearchContext context, UrlFile urlFile,
            IEnumerable<InvertedIndex> invertedIndices)
        {
            var toBeRemovedIndices = from o in context.InvertedIndices
                                     where o.UrlFileId == urlFile.UrlFileId
                                     select o;

            var toBeRemovedWordCountDict = toBeRemovedIndices.ToDictionary(o => o.Word, o => o.WordFrequency);
            var wordCountDict = invertedIndices.ToDictionary(o => o.Word, o => o.WordFrequency);
            var stats = new List<IndexStat>();

            foreach (var word in toBeRemovedWordCountDict.Keys.Union(wordCountDict.Keys))
            {
                var wordFrequencyDelta = wordCountDict.GetValueOrDefault(word, 0)
                    - toBeRemovedWordCountDict.GetValueOrDefault(word, 0);
                var docFrequencyDelta = 0;

                var hasWordBefore = toBeRemovedWordCountDict.ContainsKey(word);
                var hasWordNow = wordCountDict.ContainsKey(word);

                if (hasWordBefore && hasWordNow && wordFrequencyDelta == 0)
                {
                    continue;
                }

                if (hasWordBefore && !hasWordNow)
                {
                    docFrequencyDelta = -1;
                }
                else if (!hasWordBefore && hasWordNow)
                {
                    docFrequencyDelta = 1;
                }

                lock (mDictSyncLock)
                {
                    mIndexStatDict.AddOrUpdate(word, new IndexStatDeltaItem
                    {
                        word = word,
                        wordFrequencyDelta = wordFrequencyDelta,
                        documentFrequencyDelta = docFrequencyDelta,
                    }, (k, v) =>
                    {
                        v.wordFrequencyDelta += wordFrequencyDelta;
                        v.documentFrequencyDelta += docFrequencyDelta;
                        return v;
                    });
                }
            }
        }

        protected void FlushIndexStat()
        {
            while (true)
            {
                Thread.Sleep(10000);

                List<KeyValuePair<string, IndexStatDeltaItem>> indexStatDeltas;

                lock (mDictSyncLock)
                {
                    indexStatDeltas = mIndexStatDict
                        .ToList();
                    mIndexStatDict.Clear();
                }

                var err = indexStatDeltas.GroupBy(o => o.Value.word)
                    .Where(o => o.Count() != 1)
                    .ToList();

                var addedStats = new List<IndexStat>();
                var changedStats = new List<IndexStat>();

                int i = 0;

                using (var context = NewContext())
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                    foreach (var item in indexStatDeltas)
                    {
                        i++;
                        var word = item.Key;
                        var wordFrequencyDelta = item.Value.wordFrequencyDelta;
                        var docFrequencyDelta = item.Value.documentFrequencyDelta;

                        var stat = context.IndexStats.SingleOrDefault(o => o.Word == word);
                        if (stat == null)
                        {
                            stat = new IndexStat
                            {
                                Word = word,
                                WordFrequency = wordFrequencyDelta,
                                DocumentFrequency = docFrequencyDelta,
                            };
                            addedStats.Add(stat);
                        }
                        else
                        {
                            stat.WordFrequency += wordFrequencyDelta;
                            stat.DocumentFrequency += docFrequencyDelta;
                            if (!changedStats.Contains(stat))
                            {
                                changedStats.Add(stat);
                            }
                            else
                            {
                                changedStats.Add(stat);
                            }
                        }

                    }
                    var err2 = changedStats.GroupBy(o => o.IndexStatId)
                    .Where(o => o.Count() != 1)
                    .ToList();

                    context.BulkInsert(addedStats);
                    context.BulkUpdate(changedStats);
                    context.SaveChanges();
                }
            }
        }

        public void ClearAndSaveInvertedIndices(int urlFileId, IList<InvertedIndex> invertedIndices)
        {
            using (var context = NewContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var urlFile = context.UrlFiles.SingleOrDefault(o => o.UrlFileId == urlFileId);
                if (urlFile == null)
                {
                    return;
                }

                SaveIndexStats(context, urlFile, invertedIndices);

                var err = invertedIndices.GroupBy(o => o.Word.ToLower())
                    .Where(o => o.Count() != 1)
                    .ToList();

                ClearInvertedIndicesOf(urlFileId);
                context.BulkInsert(invertedIndices);

                urlFile.IndexStatus = UrlFile.UrlFileIndexStatus.Indexed;

                context.UrlFiles.Update(urlFile);

                lock (mSyncLock)
                {
                    context.SaveChanges();
                }
            }
        }

        public void ClearAndSaveInvertedIndices(UrlFile urlFile, IList<InvertedIndex> invertedIndices)
        {
            ClearAndSaveInvertedIndices(urlFile.UrlFileId, invertedIndices);
        }

        public void ClearInvertedIndicesOf(UrlFile urlFile)
        {
            ClearInvertedIndicesOf(urlFile.UrlFileId);
        }

        public IEnumerable<InvertedIndex> LoadByWord(string word)
        {
            using (var context = NewContext())
            {
                var indices = context.InvertedIndices.Where(o => o.Word == word);
                foreach (var index in indices)
                {
                    yield return index;
                }
            }
        }

        public InvertedIndex LoadByWordInUrlFile(int urlFileId, string word)
        {
            return mCache.Get(new CacheIndex
            {
                urlFileId = urlFileId,
                word = word,
            });
        }

        public InvertedIndex LoadByWordInUrlFile(UrlFile urlFile, string word)
        {
            return LoadByWordInUrlFile(urlFile.UrlFileId, word);
        }
    }
}
