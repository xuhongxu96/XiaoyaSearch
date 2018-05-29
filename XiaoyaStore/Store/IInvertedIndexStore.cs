using System;
using System.Collections.Generic;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface IInvertedIndexStore : IDisposable
    {
        bool ClearAndSaveIndicesOf(ulong urlFileId, ulong oldUrlFileId, IEnumerable<Index> indices);
        Index GetIndex(ulong urlFileId, string word);
    }
}