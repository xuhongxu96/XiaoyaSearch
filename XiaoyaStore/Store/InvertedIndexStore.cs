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

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : BaseStore, IInvertedIndexStore
    {
        public InvertedIndexStore(DbContextOptions options = null) : base(options)
        { }

        protected static double sStatTimeoutMinute = 1;
        protected static double sTimeoutMinute = 1;

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

                        context.RemoveRange(toBeRemovedIndices.Except(invertedIndices));
                        context.AddRange(invertedIndices.Except(toBeRemovedIndices));

                        var toBeRemovedUrlFileIndexStats = from o in context.UrlFileIndexStats
                                                           where o.UrlFileId == urlFileId
                                                           select o;

                        var urlFileIndexStats = invertedIndices.GroupBy(o => o.Word)
                            .Select(g => new UrlFileIndexStat
                            {
                                UrlFileId = urlFileId,
                                Word = g.Key,
                                WordFrequency = g.Count(),
                            });

                        context.RemoveRange(toBeRemovedUrlFileIndexStats.Except(urlFileIndexStats));
                        context.AddRange(urlFileIndexStats.Except(toBeRemovedUrlFileIndexStats));

                        foreach (var index in toBeRemovedUrlFileIndexStats)
                        {
                            var stat = context.IndexStats.SingleOrDefault(o => o.Word == index.Word);
                            if (stat != null)
                            {
                                stat.WordFrequency -= index.WordFrequency;
                                stat.DocumentFrequency--;
                            }
                        }

                        foreach (var index in urlFileIndexStats)
                        {
                            var stat = context.IndexStats.SingleOrDefault(o => o.Word == index.Word);
                            if (stat == null)
                            {
                                stat = new IndexStat
                                {
                                    Word = index.Word,
                                    WordFrequency = index.WordFrequency,
                                    DocumentFrequency = 1,
                                };
                                context.IndexStats.Add(stat);
                            }
                            else
                            {
                                stat.WordFrequency += index.WordFrequency;
                                stat.DocumentFrequency++;
                            }
                        }

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
