using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class IndexStatStore : BaseStore, IIndexStatStore
    {
        public IndexStatStore(DbContextOptions options = null) : base(options)
        { }

        public IndexStat LoadByWord(string word)
        {
            using (var context = NewContext())
            {
                return context.IndexStats.SingleOrDefault(o => o.Word == word);
            }
        }
    }
}
