using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Cache;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class UrlFileIndexStatStore : BaseStore, IUrlFileIndexStatStore
    {
        DictionaryCache<string, IReadOnlyDictionary<int, UrlFileIndexStat>> mCache;

        public UrlFileIndexStatStore(DbContextOptions options = null) : base(options)
        {
            mCache = new DictionaryCache<string, IReadOnlyDictionary<int, UrlFileIndexStat>>(
                TimeSpan.FromDays(5),
                GetCache,
                LoadCaches);
        }

        protected IEnumerable<Tuple<string, IReadOnlyDictionary<int, UrlFileIndexStat>>> LoadCaches()
        {
            using (var context = NewContext())
            {
                foreach (var item in context.UrlFileIndexStats.GroupBy(o => o.Word))
                {
                    yield return Tuple.Create(item.Key, 
                        item.ToDictionary(o => o.UrlFileId, o => o) as IReadOnlyDictionary<int, UrlFileIndexStat>);
                }

            }
        }

        protected IReadOnlyDictionary<int, UrlFileIndexStat> GetCache(string word)
        {
            using (var context = NewContext())
            {
                return context.UrlFileIndexStats
                    .Where(o => o.Word == word)
                    .ToDictionary(o => o.UrlFileId, o => o);
            }
        }

        public IEnumerable<UrlFileIndexStat> LoadByWord(string word)
        {
            foreach (var item in mCache.Get(word))
            {
                yield return item.Value;
            }
        }

        public UrlFileIndexStat LoadByWordInUrlFile(UrlFile urlFile, string word)
        {
            return LoadByWordInUrlFile(urlFile.UrlFileId, word);
        }

        public UrlFileIndexStat LoadByWordInUrlFile(int urlFileId, string word)
        {
            return mCache.Get(word)[urlFileId];
        }

        public int CountWordInUrlFile(int urlFileId)
        {
            using (var context = NewContext())
            {
                return context.UrlFileIndexStats
                    .Where(o => o.UrlFileId == urlFileId)
                    .Count();
            }
        }

        public int CountWordInUrlFile(UrlFile urlFile)
        {
            return CountWordInUrlFile(urlFile.UrlFileId);
        }
    }
}
