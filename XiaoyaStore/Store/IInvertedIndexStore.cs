using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IInvertedIndexStore
    {
        Task ClearInvertedIndicesFor(UrlFile urlFile);
        IEnumerable<InvertedIndex> LoadByUrlFilePosition(int urlFileId, int position);
        IEnumerable<InvertedIndex> LoadByUrlFilePosition(UrlFile urlFile, int position);
        IEnumerable<InvertedIndex> LoadByWord(string word);
        Task SaveInvertedIndexAsync(InvertedIndex invertedIndex);
        Task SaveInvertedIndicesAsync(IEnumerable<InvertedIndex> invertedIndices);
    }
}