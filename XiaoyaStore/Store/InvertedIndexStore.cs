using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : IInvertedIndexStore
    {
        protected XiaoyaSearchContext mContext;
        public InvertedIndexStore(XiaoyaSearchContext context)
        {
            mContext = context;
        }

        public async Task SaveInvertedIndexAsync(InvertedIndex invertedIndex)
        {
            await mContext.InvertedIndices.AddAsync(invertedIndex);
            await mContext.SaveChangesAsync();
        }

        public async Task SaveInvertedIndicesAsync(IEnumerable<InvertedIndex> invertedIndices)
        {
            await mContext.InvertedIndices.AddRangeAsync(invertedIndices);
            await mContext.SaveChangesAsync();
        }

        public async Task ClearInvertedIndicesOf(UrlFile urlFile)
        {
            mContext.RemoveRange(from o in mContext.InvertedIndices
                                 where o.UrlFileId == urlFile.UrlFileId
                                 select o);
            await mContext.SaveChangesAsync();
        }

        public IEnumerable<InvertedIndex> LoadByWord(string word)
        {
            return mContext.InvertedIndices.Where(o => o.Word == word).AsEnumerable();
        }

        public InvertedIndex LoadByUrlFilePosition(int urlFileId, int position)
        {
            return mContext.InvertedIndices
                .Where(o => o.UrlFileId == urlFileId && o.Position <= position)
                .OrderByDescending(o => o.Position)
                .FirstOrDefault();
        }

        public InvertedIndex LoadByUrlFilePosition(UrlFile urlFile, int position)
        {
            return LoadByUrlFilePosition(urlFile.UrlFileId, position);
        }
    }
}
