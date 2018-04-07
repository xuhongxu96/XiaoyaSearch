using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;
using static XiaoyaStore.Data.Model.InvertedIndex;

namespace XiaoyaStore.Store
{
    public interface IInvertedIndexStore
    {
        void ClearInvertedIndicesOf(UrlFile urlFile);
        InvertedIndex LoadByUrlFilePosition(int urlFileId, int position, InvertedIndexType indexType = InvertedIndexType.Body);
        InvertedIndex LoadByUrlFilePosition(UrlFile urlFile, int position, InvertedIndexType indexType = InvertedIndexType.Body);
        IEnumerable<InvertedIndex> LoadByWord(string word, InvertedIndexType indexType = InvertedIndexType.Body);
        IEnumerable<InvertedIndex> LoadByWordInUrlFileOrderByPosition(int urlFileId, string word, InvertedIndexType indexType = InvertedIndexType.Body);
        IEnumerable<InvertedIndex> LoadByWordInUrlFileOrderByPosition(UrlFile urlFile, string word, InvertedIndexType indexType = InvertedIndexType.Body);
        void ClearAndSaveInvertedIndices(UrlFile urlFile, IEnumerable<InvertedIndex> invertedIndices);
        void ClearAndSaveInvertedIndices(int urlFileId, IEnumerable<InvertedIndex> invertedIndices);
        int CountTitleIndexInUrlFile(int urlFileId);
        int CountTitleIndexInUrlFile(UrlFile urlFile);
    }
}