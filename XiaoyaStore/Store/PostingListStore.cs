using System;
using System.Collections.Generic;
using System.Text;
using RocksDbSharp;
using XiaoyaStore.Config;
using XiaoyaStore.Data.MergeOperator;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Cache;
using XiaoyaStore.Data;

namespace XiaoyaStore.Store
{
    public class PostingListStore : BaseStore
    {
        public override string DbFileName => "PostingList";

        protected LRUCache<string, PostingList> mCache;

        public PostingListStore(StoreConfig config,
            bool isReadOnly = false,
            bool enableCache = true)
            : base(config, isReadOnly)
        {
            OpenDb();
            mCache = new LRUCache<string, PostingList>(TimeSpan.FromDays(1), GetCache, LoadCaches, 1_000_000, enableCache);
        }

        private IEnumerable<Tuple<string, PostingList>> LoadCaches()
        {
            var iter = mDb.NewIterator();
            for (iter.SeekToFirst(); iter.Valid(); iter.Next())
            {
                var data = iter.Value();
                var item = ModelSerializer.DeserializeModel<PostingList>(data);
                yield return Tuple.Create(item.Word, item);
            }
        }

        private PostingList GetCache(string word)
        {
            var data = mDb.Get(word.GetBytes());
            if (data == null)
            {
                return null;
            }
            return ModelSerializer.DeserializeModel<PostingList>(data);
        }

        public PostingList LoadByWord(string word)
        {
            return mCache.Get(word);
        }

        public void SavePostingList(PostingList deltaPostingList)
        {
            var data = ModelSerializer.SerializeModel(deltaPostingList);
            mDb.Merge(deltaPostingList.Word.GetBytes(), data);
        }
    }
}
