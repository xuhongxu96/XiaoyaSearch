using System;
using System.Collections.Generic;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface IInvertedIndexStore : IDisposable
    {
        bool ClearIndices(ulong urlFileId);
        bool SaveIndices(ulong urlFileId, IEnumerable<Index> indices);
        Index GetIndex(ulong urlFileId, string word);
    }
}