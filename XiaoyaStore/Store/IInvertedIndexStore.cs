using System.Collections.Generic;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IInvertedIndexStore
    {
        void ClearAndSaveInvertedIndices(int urlFileId, IList<InvertedIndex> invertedIndices);
        void ClearAndSaveInvertedIndices(UrlFile urlFile, IList<InvertedIndex> invertedIndices);
        void ClearInvertedIndicesOf(int urlFileId);
        void ClearInvertedIndicesOf(UrlFile urlFile);
        IEnumerable<int> LoadUrlFileIdsByWord(string word, double minWeight = 0);
        InvertedIndex LoadByWordInUrlFile(int urlFileId, string word);
        InvertedIndex LoadByWordInUrlFile(UrlFile urlFile, string word);
        void CacheWordsInUrlFiles(IEnumerable<int> urlFileIds, IEnumerable<string> words);
    }
}