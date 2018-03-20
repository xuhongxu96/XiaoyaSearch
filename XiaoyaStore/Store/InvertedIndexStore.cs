using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : BaseStore, IInvertedIndexStore
    {
        public InvertedIndexStore(DbContextOptions options = null) : base(options)
        { }

        public void SaveInvertedIndex(InvertedIndex invertedIndex)
        {
            using (var context = NewContext())
            {
                context.InvertedIndices.Add(invertedIndex);
                context.SaveChanges();
            }
        }

        public void SaveInvertedIndices(IEnumerable<InvertedIndex> invertedIndices)
        {
            using (var context = NewContext())
            {
                context.InvertedIndices.AddRange(invertedIndices);
                context.SaveChanges();
            }
        }

        public void ClearInvertedIndicesOf(UrlFile urlFile)
        {
            using (var context = NewContext())
            {
                context.RemoveRange(from o in context.InvertedIndices
                                    where o.UrlFileId == urlFile.UrlFileId
                                    select o);
                context.SaveChanges();
            }
        }

        public IEnumerable<InvertedIndex> LoadByWord(string word)
        {
            using (var context = NewContext())
            {
                foreach (var index in context.InvertedIndices.Where(o => o.Word == word))
                {
                    yield return index;
                }
            }
        }

        public InvertedIndex LoadByUrlFilePosition(int urlFileId, int position)
        {
            using (var context = NewContext())
            {
                return context.InvertedIndices
                .Where(o => o.UrlFileId == urlFileId && o.Position <= position)
                .OrderByDescending(o => o.Position)
                .FirstOrDefault();
            }
        }

        public InvertedIndex LoadByUrlFilePosition(UrlFile urlFile, int position)
        {
            return LoadByUrlFilePosition(urlFile.UrlFileId, position);
        }
    }
}
