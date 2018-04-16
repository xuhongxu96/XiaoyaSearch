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

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : BaseStore, IInvertedIndexStore
    {
        protected object mSyncLock = new object();

        public struct CacheIndex
        {
            public int urlFileId;
            public string word;
            public InvertedIndexType indexType;
        }

        protected LRUCache<CacheIndex, IReadOnlyList<InvertedIndex>> mCache;

        public InvertedIndexStore(DbContextOptions options = null, bool enableCache = true) : base(options)
        {
                mCache = new LRUCache<CacheIndex, IReadOnlyList<InvertedIndex>>(
                    TimeSpan.FromDays(5), GetCache, LoadCaches, 100_000_000, enableCache);
        }

        protected IEnumerable<Tuple<CacheIndex, IReadOnlyList<InvertedIndex>>> LoadCaches()
        {
            using (var context = NewContext())
            {
                foreach (var item in context.InvertedIndices
                .GroupBy(o => new CacheIndex
                {
                    urlFileId = o.UrlFileId,
                    word = o.Word,
                    indexType = o.IndexType,
                }))
                {
                    yield return Tuple.Create(item.Key, item.ToList().AsReadOnly() as IReadOnlyList<InvertedIndex>);
                }
            }
        }

        protected IReadOnlyList<InvertedIndex> GetCache(CacheIndex cacheIndex)
        {
            using (var context = NewContext())
            {
                return context.InvertedIndices
                   .Where(o => o.UrlFileId == cacheIndex.urlFileId)
                   .Where(o => o.Word == cacheIndex.word)
                   .Where(o => o.IndexType == cacheIndex.indexType)
                   .OrderBy(o => o.Position)
                   .ToList();
            }
        }

        protected double CalculateWeight(UrlFile urlFile, string word, long wordFrequency, int minPosition)
        {
            var title = urlFile.Title;
            if (title == null)
            {
                title = "";
            }

            var content = urlFile.Content;
            if (content == null)
            {
                content = "";
            }

            return (title.Contains(word) ? 2.0 * word.Length : 1.0)
                / (1 + title.Length)
                * wordFrequency
                / (1 + 10 * UrlHelper.GetDomainDepth(urlFile.Url))
                + 3 * (minPosition / (1 + content.Length));
        }

        public void ClearAndSaveInvertedIndices(int urlFileId, IEnumerable<InvertedIndex> invertedIndices)
        {
            using (var context = NewContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
    
                var urlFile = context.UrlFiles.SingleOrDefault(o => o.UrlFileId == urlFileId);
                if (urlFile == null)
                {
                    return;
                }

                context.Database.ExecuteSqlCommand($"DELETE FROM InvertedIndices WHERE UrlFileId = {urlFileId}");
                context.Database.ExecuteSqlCommand($"DELETE FROM UrlFileIndexStats WHERE UrlFileId = {urlFileId}");

                context.InvertedIndices.AddRange(invertedIndices);

                var urlFileIndexStats = invertedIndices
                    .GroupBy(o => o.Word)
                    .Select(g => new UrlFileIndexStat
                    {
                        UrlFileId = urlFileId,
                        Word = g.Key,
                        WordFrequency = g.Count(),
                        Weight = CalculateWeight(urlFile, g.Key, g.Count(), g.Min(o => o.Position)),
                    });

                context.UrlFileIndexStats.AddRange(urlFileIndexStats);

                if (!context.Database.IsSqlServer())
                {

                    var toBeRemovedIndices = from o in context.InvertedIndices
                                             where o.UrlFileId == urlFileId
                                             select o;
                    var toBeRemovedUrlFileIndexStats = from o in context.UrlFileIndexStats
                                                       where o.UrlFileId == urlFileId
                                                       select o;

                    lock (mSyncLock)
                    {
                        var toBeRemovedWordCountDict = toBeRemovedUrlFileIndexStats.ToDictionary(o => o.Word, o => o.WordFrequency);
                        var wordCountDict = urlFileIndexStats.ToDictionary(o => o.Word, o => o.WordFrequency);

                        foreach (var word in toBeRemovedWordCountDict.Keys.Union(wordCountDict.Keys))
                        {
                            var frequencyDelta = wordCountDict.GetValueOrDefault(word, 0)
                                - toBeRemovedWordCountDict.GetValueOrDefault(word, 0);

                            var hasWordBefore = toBeRemovedWordCountDict.ContainsKey(word);
                            var hasWordNow = wordCountDict.ContainsKey(word);

                            if (hasWordBefore && hasWordNow && frequencyDelta == 0)
                            {
                                continue;
                            }

                            var stat = context.IndexStats.SingleOrDefault(o => o.Word == word);

                            if (stat == null)
                            {
                                if (frequencyDelta < 0 || hasWordBefore)
                                {
                                    continue;
                                }

                                stat = new IndexStat
                                {
                                    Word = word,
                                    WordFrequency = 0,
                                    DocumentFrequency = 0,
                                };

                                context.IndexStats.Add(stat);
                            }

                            if (hasWordBefore && !hasWordNow)
                            {
                                stat.DocumentFrequency--;
                            }
                            else if (!hasWordBefore && hasWordNow)
                            {
                                stat.DocumentFrequency++;
                            }

                            stat.WordFrequency += frequencyDelta;
                        }

                        context.UrlFiles.Single(o => o.UrlFileId == urlFileId).IndexStatus
                            = UrlFile.UrlFileIndexStatus.Indexed;

                        context.SaveChanges();

                        return;
                    }
                }

                context.UrlFiles.Single(o => o.UrlFileId == urlFileId).IndexStatus
                = UrlFile.UrlFileIndexStatus.Indexed;

                lock (mSyncLock)
                {
                    context.SaveChanges();
                }
            }

        }

        public void ClearAndSaveInvertedIndices(UrlFile urlFile, IEnumerable<InvertedIndex> invertedIndices)
        {
            ClearAndSaveInvertedIndices(urlFile.UrlFileId, invertedIndices);
        }

        public void ClearInvertedIndicesOf(UrlFile urlFile)
        {
            using (var context = NewContext())
            {
                var toBeRemovedIndices = from o in context.InvertedIndices
                                         where o.UrlFileId == urlFile.UrlFileId
                                         select o;

                context.RemoveRange(toBeRemovedIndices);

                var toBeRemovedUrlFileIndices = from o in context.UrlFileIndexStats
                                                where o.UrlFileId == urlFile.UrlFileId
                                                select o;

                context.RemoveRange(toBeRemovedUrlFileIndices);
                context.SaveChanges();
            }
        }

        public IEnumerable<InvertedIndex> LoadByWord(string word, InvertedIndexType indexType = InvertedIndexType.Body)
        {
            using (var context = NewContext())
            {
                var indices = context.InvertedIndices.Where(o => o.Word == word && o.IndexType == indexType);
                foreach (var index in indices)
                {
                    yield return index;
                }
            }
        }

        public IEnumerable<InvertedIndex> LoadByWordInUrlFileOrderByPosition(int urlFileId, string word, InvertedIndexType indexType = InvertedIndexType.Body)
        {
            return mCache.Get(new CacheIndex
            {
                urlFileId = urlFileId,
                word = word,
                indexType = indexType,
            });
        }

        public IEnumerable<InvertedIndex> LoadByWordInUrlFileOrderByPosition(UrlFile urlFile, string word, InvertedIndexType indexType = InvertedIndexType.Body)
        {
            return LoadByWordInUrlFileOrderByPosition(urlFile.UrlFileId, word);
        }

        public InvertedIndex LoadByUrlFilePosition(int urlFileId, int position, InvertedIndexType indexType = InvertedIndexType.Body)
        {
            using (var context = NewContext())
            {
                return context.InvertedIndices
                    .Where(o => o.UrlFileId == urlFileId && o.IndexType == indexType && o.Position <= position)
                    .OrderByDescending(o => o.Position)
                    .FirstOrDefault();
            }
        }

        public InvertedIndex LoadByUrlFilePosition(UrlFile urlFile, int position, InvertedIndexType indexType = InvertedIndexType.Body)
        {
            return LoadByUrlFilePosition(urlFile.UrlFileId, position);
        }
    }
}
