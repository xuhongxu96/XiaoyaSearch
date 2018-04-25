﻿using System;
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
        private const int BatchSize = 500000;

        private object mSyncLock = new object();
        private object mDictSyncLock = new object();

        private object mSavingIndexStatLock = new object();
        private bool mSavingIndexStat = false;

        private RuntimeLogger mLogger = null;

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

        public struct CacheWord
        {
            public string word;
            public double minWeight;

            public override bool Equals(object obj)
            {
                if (!(obj is CacheWord))
                {
                    return false;
                }

                var word = (CacheWord)obj;
                return this.word == word.word;
            }

            public override int GetHashCode()
            {
                var hashCode = 1788406269;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(word);
                return hashCode;
            }
        }

        public struct CacheWordItem
        {
            public double minWeight;
            public List<int> urlFileIds;
        }

        public struct IndexStatDeltaItem
        {
            public string word;
            public long wordFrequencyDelta;
            public int documentFrequencyDelta;
        }

        protected LRUCache<CacheIndex, InvertedIndex> mWordUrlFileCache;
        protected ComparableLRUCache<CacheWord, CacheWordItem> mWordCache;
        protected ConcurrentDictionary<string, IndexStatDeltaItem> mIndexStatDict = new ConcurrentDictionary<string, IndexStatDeltaItem>();

        public InvertedIndexStore(DbContextOptions options = null, bool enableCache = true, RuntimeLogger logger = null) : base(options)
        {
            mWordUrlFileCache = new LRUCache<CacheIndex, InvertedIndex>(
                TimeSpan.FromDays(5), GetWordUrlFileCache, LoadWordUrlFileCaches, 30_000_000, enableCache);

            mWordCache = new ComparableLRUCache<CacheWord, CacheWordItem>(
                TimeSpan.FromDays(1), GetWordCache, UpdateWordCache, CompareWordCache, LoadWordCaches, 1_000_000, enableCache);

            mLogger = logger;

            var thread = new Thread(FlushIndexStat)
            {
                Priority = ThreadPriority.AboveNormal,
            };
            thread.Start();
        }

        private bool CompareWordCache(CacheWord word, CacheWordItem value)
        {
            return word.minWeight >= value.minWeight;
        }

        private CacheWordItem UpdateWordCache(CacheWord word, CacheWordItem value)
        {
            using (var context = NewContext())
            {
                var result = context.InvertedIndices.Where(o => o.Word == word.word)
                    .Where(o => o.Weight >= word.minWeight)
                    .Where(o => o.Weight < value.minWeight)
                    .OrderByDescending(o => o.Weight)
                    .Select(o => o.UrlFileId)
                    .ToList();

                value.urlFileIds.AddRange(result);
                value.minWeight = word.minWeight;

                return value;
            }
        }

        private IEnumerable<Tuple<CacheWord, CacheWordItem>> LoadWordCaches()
        {
            using (var context = NewContext())
            {
                foreach (var stat in context.IndexStats.OrderByDescending(o => o.WordFrequency))
                {
                    if (mWordCache.IsValid(new CacheWord
                    {
                        word = stat.Word,
                        minWeight = 2,
                    }))
                    {
                        continue;
                    }

                    var result = context.InvertedIndices.Where(o => o.Word == stat.Word)
                    .Where(o => o.Weight >= 2)
                    .OrderByDescending(o => o.Weight)
                    .Select(o => o.UrlFileId)
                    .ToList();

                    yield return Tuple.Create(new CacheWord
                    {
                        word = stat.Word,
                        minWeight = 2,
                    }, new CacheWordItem
                    {
                        urlFileIds = result,
                        minWeight = 2,
                    });
                }
            }
        }

        private CacheWordItem GetWordCache(CacheWord word)
        {
            using (var context = NewContext())
            {
                var result = context.InvertedIndices.Where(o => o.Word == word.word)
                    .Where(o => o.Weight >= word.minWeight)
                    .OrderByDescending(o => o.Weight)
                    .Select(o => o.UrlFileId)
                    .ToList();

                return new CacheWordItem
                {
                    urlFileIds = result,
                    minWeight = word.minWeight,
                };
            }
        }

        public void CacheWordsInUrlFiles(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var wordSet = words.ToHashSet();
            foreach (var word in words)
            {
                bool allValid = true;
                foreach (var id in urlFileIds)
                {
                    if (!mWordUrlFileCache.IsValid(new CacheIndex
                    {
                        word = word,
                        urlFileId = id,
                    }))
                    {
                        allValid = false;
                    }
                }
                if (allValid)
                {
                    wordSet.Remove(word);
                }
            }
            mWordUrlFileCache.LoadCaches(() => LoadCachesOfWordsInUrlFiles(urlFileIds, wordSet));
        }

        private IEnumerable<Tuple<CacheIndex, InvertedIndex>> LoadCachesOfWordsInUrlFiles(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            using (var context = NewContext())
            {
                foreach (var item in context.InvertedIndices
                    .Where(o => urlFileIds.Contains(o.UrlFileId) && words.Contains(o.Word))
                    .OrderByDescending(o => o.Weight))
                {
                    if (mWordUrlFileCache.IsValid(new CacheIndex
                    {
                        word = item.Word,
                        urlFileId = item.UrlFileId,
                    }))
                    {
                        continue;
                    }

                    yield return Tuple.Create(new CacheIndex
                    {
                        urlFileId = item.UrlFileId,
                        word = item.Word,
                    }, item);
                }
            }
        }

        private IEnumerable<Tuple<CacheIndex, InvertedIndex>> LoadWordUrlFileCaches()
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

        private InvertedIndex GetWordUrlFileCache(CacheIndex cacheIndex)
        {
            using (var context = NewContext())
            {
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
                var list = new List<InvertedIndex>();
                foreach (var item in context.InvertedIndices.Where(o => o.UrlFileId == urlFileId))
                {
                    list.Add(item);
                    if (list.Count % BatchSize == 0)
                    {
                        if (context.Database.IsSqlServer())
                        {
                            context.BulkDelete(list);
                        }
                        else
                        {
                            context.RemoveRange(list);
                        }
                        list.Clear();
                    }
                }
                if (list.Count > 0)
                {
                    if (context.Database.IsSqlServer())
                    {
                        context.BulkDelete(list);
                    }
                    else
                    {
                        context.RemoveRange(list);
                    }
                }
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
                Thread.Sleep(5000);

                List<KeyValuePair<string, IndexStatDeltaItem>> indexStatDeltas;

                lock (mSavingIndexStatLock)
                {
                    mSavingIndexStat = true;
                }

                lock (mDictSyncLock)
                {
                    indexStatDeltas = mIndexStatDict
                        .ToList();
                    mIndexStatDict.Clear();
                }

                var addedStats = new HashSet<IndexStat>();
                var changedStats = new HashSet<IndexStat>();

                int i = 0;

                using (var context = NewContext())
                {
                    while (true)
                    {
                        try
                        {
                            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
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
                                    if (!addedStats.Contains(stat))
                                    {
                                        addedStats.Add(stat);
                                    }
                                }
                                else
                                {
                                    stat.WordFrequency += wordFrequencyDelta;
                                    stat.DocumentFrequency += docFrequencyDelta;
                                    if (!changedStats.Contains(stat))
                                    {
                                        changedStats.Add(stat);
                                    }
                                }
                            }

                            if (context.Database.IsSqlServer())
                            {
                                context.BulkInsert(addedStats.ToList());
                                context.BulkUpdate(changedStats.ToList());
                            }
                            else
                            {
                                context.AddRange(addedStats.ToList());
                                context.UpdateRange(changedStats.ToList());
                            }
                            
                            context.SaveChanges();
                            break;
                        }
                        catch (Exception e)
                        {
                            mLogger?.LogException(nameof(InvertedIndexStore), "Error while saving index stats", e);
                        }
                    }
                }

                lock (mSavingIndexStatLock)
                {
                    mSavingIndexStat = false;
                }
            }
        }

        public void ClearAndSaveInvertedIndices(int urlFileId, IList<InvertedIndex> invertedIndices)
        {
            using (var context = NewContext())
            {
                context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
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

                var list = new List<InvertedIndex>();
                foreach (var index in invertedIndices)
                {
                    list.Add(index);
                    if (list.Count % BatchSize == 0)
                    {
                        if (context.Database.IsSqlServer())
                        {
                            context.BulkInsert(list);
                        }
                        else
                        {
                            context.AddRange(list);
                        }
                        list.Clear();
                    }
                }
                if (list.Count > 0)
                {
                    if (context.Database.IsSqlServer())
                    {
                        context.BulkInsert(list);
                    }
                    else
                    {
                        context.AddRange(list);
                    }
                }
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

        public IEnumerable<int> LoadUrlFileIdsByWord(string word, double minWeight = 0)
        {
            return mWordCache.Get(new CacheWord
            {
                word = word,
                minWeight = minWeight,
            }).urlFileIds;
        }

        public InvertedIndex LoadByWordInUrlFile(int urlFileId, string word)
        {
            return mWordUrlFileCache.Get(new CacheIndex
            {
                urlFileId = urlFileId,
                word = word,
            });
        }

        public InvertedIndex LoadByWordInUrlFile(UrlFile urlFile, string word)
        {
            return LoadByWordInUrlFile(urlFile.UrlFileId, word);
        }

        public void WaitForIndexStat()
        {
            while (!mIndexStatDict.IsEmpty || mSavingIndexStat)
            {
                Thread.Sleep(3000);
            }
        }
    }
}
