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

                var stat = context.IndexStats.SingleOrDefault(o => o.Word == invertedIndex.Word);
                if (stat == null)
                {
                    stat = new IndexStat
                    {
                        Word = invertedIndex.Word,
                        Count = 1,
                    };
                    context.IndexStats.Add(stat);
                }
                else
                {
                    stat.Count++;
                    if (stat.Count > long.MaxValue / 2)
                    {
                        stat.Count = long.MaxValue / 2;
                    }
                }

                context.SaveChanges();
            }
        }

        public void SaveInvertedIndices(IEnumerable<InvertedIndex> invertedIndices)
        {
            using (var context = NewContext())
            {
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
                            Count = index.Count,
                        };
                        context.IndexStats.Add(stat);
                    }
                    else
                    {
                        stat.Count += index.Count;
                    }

                    if (stat.Count > long.MaxValue / 2)
                    {
                        stat.Count = long.MaxValue / 2;
                    }
                }

                context.SaveChanges();
            }
        }

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
                    if (stat != null && stat.Count > 0)
                    {
                        stat.Count -= index.Count;
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
                            Count = index.Count,
                        };
                        context.IndexStats.Add(stat);
                    }
                    else
                    {
                        stat.Count += index.Count;
                    }

                    if (stat.Count > long.MaxValue / 2)
                    {
                        stat.Count = long.MaxValue / 2;
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
                    if (stat != null && stat.Count > 0)
                    {
                        stat.Count -= index.Count;
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
