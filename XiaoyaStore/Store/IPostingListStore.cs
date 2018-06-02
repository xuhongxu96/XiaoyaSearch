using System;
using System.Collections.Generic;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface IPostingListStore : IDisposable
    {
        PostingList GetPostingList(string word);
        bool SavePostingLists(ulong urlFileId, IEnumerable<PostingList> postingLists);
        bool ClearPostingLists(ulong urlFileId);
    }
}