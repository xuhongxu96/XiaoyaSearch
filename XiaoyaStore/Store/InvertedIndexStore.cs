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

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : BaseStore, IInvertedIndexStore
    {
        public InvertedIndexStore(DbContextOptions options = null) : base(options)
        { }

        public void ClearAndSaveInvertedIndices(int urlFileId, IEnumerable<InvertedIndex> invertedIndices)
        {

            try
            {
                using (var context = NewContext())
                {
                    var toBeRemovedIndices = from o in context.InvertedIndices
                                             where o.UrlFileId == urlFileId
                                             select o;
                    var toBeRemovedUrlFileIndexStats = from o in context.UrlFileIndexStats
                                                       where o.UrlFileId == urlFileId
                                                       select o;
                    context.RemoveRange(toBeRemovedIndices);
                    context.RemoveRange(toBeRemovedUrlFileIndexStats);

                    context.InvertedIndices.AddRange(invertedIndices);

                    var urlFileIndexStats = invertedIndices
                        .GroupBy(o => o.Word)
                        .Select(g => new UrlFileIndexStat
                        {
                            UrlFileId = urlFileId,
                            Word = g.Key,
                            WordFrequency = g.Count(),
                        });

                    context.UrlFileIndexStats.AddRange(urlFileIndexStats);

                    if (!context.Database.IsSqlServer())
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

                    }

                    context.UrlFiles.Single(o => o.UrlFileId == urlFileId).IndexStatus
                    = UrlFile.UrlFileIndexStatus.Indexed;

                    context.SaveChanges();
                }
            }
            catch (Exception)
            {
                using (var context = NewContext())
                {
                    context.UrlFiles.Single(o => o.UrlFileId == urlFileId).IndexStatus
                        = UrlFile.UrlFileIndexStatus.NotIndexed;

                    context.SaveChanges();
                }
                throw;
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
            using (var context = NewContext())
            {
                var indices = context.InvertedIndices
                    .Where(o => o.UrlFileId == urlFileId && o.Word == word && o.IndexType == indexType)
                    .OrderBy(o => o.Position);

                foreach (var index in indices)
                {
                    yield return index;
                }
            }
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
