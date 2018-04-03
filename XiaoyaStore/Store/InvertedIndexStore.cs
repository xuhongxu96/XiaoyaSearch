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

        protected static double sStatTimeoutMinute = 1;
        protected static double sTimeoutMinute = 1;

        public void GenerateStat()
        {
            using (var context = NewContext())
            {
                while (true)
                {
                    try
                    {
                        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(sStatTimeoutMinute));
                        if (context.Database.IsSqlServer())
                        {
                            context.Database.ExecuteSqlCommand(@"
TRUNCATE TABLE IndexStats;
INSERT INTO IndexStats (Word, DocumentFrequency, WordFrequency) SELECT Word AS Word, COUNT(DISTINCT UrlFileId) AS DocumentFrequency, COUNT(*) AS WordFrequency FROM InvertedIndices GROUP BY Word;
TRUNCATE TABLE UrlFileIndexStats;
INSERT INTO UrlFileIndexStats (Word, UrlFileId, WordFrequency) SELECT Word AS Word, UrlFileId AS UrlFileId, COUNT(*) AS WordFrequency FROM InvertedIndices GROUP BY Word, UrlFileId;");
                        }
                        else
                        {
                            context.RemoveRange(context.IndexStats);
                            context.RemoveRange(context.UrlFileIndexStats);

                            context.SaveChanges();

                            context.Database.ExecuteSqlCommand(@"
INSERT INTO IndexStats (Word, DocumentFrequency, WordFrequency) SELECT Word AS Word, COUNT(DISTINCT UrlFileId) AS DocumentFrequency, COUNT(*) AS WordFrequency FROM InvertedIndices GROUP BY Word;
INSERT INTO UrlFileIndexStats (Word, UrlFileId, WordFrequency) SELECT Word AS Word, UrlFileId AS UrlFileId, COUNT(*) AS WordFrequency FROM InvertedIndices GROUP BY Word, UrlFileId;");
                        }
                        break;
                    }
                    catch (SqlException e) when (e.Message.Contains("timeout"))
                    {
                        sStatTimeoutMinute *= 2;
                        if (sStatTimeoutMinute > 30)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public void ClearAndSaveInvertedIndices(int urlFileId, IEnumerable<InvertedIndex> invertedIndices)
        {
            using (var context = NewContext())
            {
                while (true)
                {
                    try
                    {
                        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(sTimeoutMinute));

                        var toBeRemovedIndices = from o in context.InvertedIndices
                                                 where o.UrlFileId == urlFileId
                                                 select o;

                        context.RemoveRange(toBeRemovedIndices);
                        context.InvertedIndices.AddRange(invertedIndices);

                        context.UrlFiles.Single(o => o.UrlFileId == urlFileId).IndexStatus
                            = UrlFile.UrlFileIndexStatus.Indexed;

                        context.SaveChanges();

                        break;
                    }
                    catch (SqlException e) when (e.Message.Contains("timeout"))
                    {
                        sTimeoutMinute *= 2;
                        if (sTimeoutMinute > 30)
                        {
                            throw;
                        }
                    }
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
