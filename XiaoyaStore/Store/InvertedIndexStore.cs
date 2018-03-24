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

        public void ClearAndSaveInvertedIndices(UrlFile urlFile, IEnumerable<InvertedIndex> invertedIndices)
        {
            using (var context = NewContext())
            {
                var toBeRemovedIndices = from o in context.InvertedIndices
                                         where o.UrlFileId == urlFile.UrlFileId
                                         select o;

                var groupedRemovedIndices = toBeRemovedIndices.GroupBy(o => o.Word)
                    .Select(g => new
                    {
                        Word = g.Key,
                        Count = g.Count(),
                    });

                foreach (var index in groupedRemovedIndices)
                {
                    var stat = context.IndexStats.SingleOrDefault(o => o.Word == index.Word);
                    if (stat != null && stat.WordFrequency > 0)
                    {
                        stat.WordFrequency -= index.Count;
                        stat.DocumentFrequency--;
                    }
                }

                context.RemoveRange(toBeRemovedIndices);
                context.InvertedIndices.AddRange(invertedIndices);

                var groupedIndices = invertedIndices.GroupBy(o => o.Word)
                    .Select(g => new
                    {
                        Word = g.Key,
                        Count = g.Count(),
                    });

                foreach (var index in groupedIndices)
                {
                    var stat = context.IndexStats.SingleOrDefault(o => o.Word == index.Word);
                    if (stat == null)
                    {
                        stat = new IndexStat
                        {
                            Word = index.Word,
                            WordFrequency = index.Count,
                            DocumentFrequency = 1,
                        };
                        context.IndexStats.Add(stat);
                    }
                    else
                    {
                        stat.WordFrequency += index.Count;
                        stat.DocumentFrequency++;
                    }

                    if (stat.WordFrequency > long.MaxValue / 2)
                    {
                        stat.WordFrequency = long.MaxValue / 2;
                    }

                    if (stat.DocumentFrequency > long.MaxValue - 1)
                    {
                        stat.DocumentFrequency = long.MaxValue - 1;
                    }
                }

                context.SaveChanges();
            }
        }

        public void ClearInvertedIndicesOf(UrlFile urlFile)
        {
            using (var context = NewContext())
            {
                var toBeRemovedIndices = from o in context.InvertedIndices
                                         where o.UrlFileId == urlFile.UrlFileId
                                         select o;
                var groupedRemovedIndices = toBeRemovedIndices.GroupBy(o => o.Word)
                    .Select(g => new
                    {
                        Word = g.Key,
                        Count = g.Count(),
                    });

                foreach (var index in groupedRemovedIndices)
                {
                    var stat = context.IndexStats.SingleOrDefault(o => o.Word == index.Word);
                    if (stat != null && stat.WordFrequency > 0)
                    {
                        stat.WordFrequency -= index.Count;
                    }
                }

                context.RemoveRange(toBeRemovedIndices);
                context.SaveChanges();
            }
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
