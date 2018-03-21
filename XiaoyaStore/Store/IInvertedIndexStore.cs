using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IInvertedIndexStore
    {
        void ClearInvertedIndicesOf(UrlFile urlFile);
        InvertedIndex LoadByUrlFilePosition(int urlFileId, int position);
        InvertedIndex LoadByUrlFilePosition(UrlFile urlFile, int position);
        IEnumerable<InvertedIndex> LoadByWord(string word);
        void SaveInvertedIndex(InvertedIndex invertedIndex);
        void SaveInvertedIndices(IEnumerable<InvertedIndex> invertedIndices);
        void ClearAndSaveInvertedIndices(UrlFile urlFile, IEnumerable<InvertedIndex> invertedIndices);
    }
}