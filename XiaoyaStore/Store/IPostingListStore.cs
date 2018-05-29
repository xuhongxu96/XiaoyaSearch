using System;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface IPostingListStore : IDisposable
    {
        PostingList GetPostingList(string word);
        bool SavePostingList(PostingList postingList);
    }
}