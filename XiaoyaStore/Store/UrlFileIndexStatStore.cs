using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class UrlFileIndexStatStore : BaseStore, IUrlFileIndexStatStore
    {
        public UrlFileIndexStatStore(DbContextOptions options = null) : base(options)
        { }

        public IEnumerable<UrlFileIndexStat> LoadByWord(string word)
        {
            using (var context = NewContext())
            {
                foreach (var stat in context.UrlFileIndexStats
                    .Where(o => o.Word == word)
                    .OrderByDescending(o => o.WordFrequency))
                {
                    yield return stat;
                }
            }
        }

        public UrlFileIndexStat LoadByWordInUrlFile(UrlFile urlFile, string word)
        {
            return LoadByWordInUrlFile(urlFile.UrlFileId, word);
        }

        public UrlFileIndexStat LoadByWordInUrlFile(int urlFileId, string word)
        {
            using (var context = NewContext())
            {
                return context.UrlFileIndexStats
                    .SingleOrDefault(o => o.Word == word && o.UrlFileId == urlFileId);
            }
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
