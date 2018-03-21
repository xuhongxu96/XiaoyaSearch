using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever
{
    public class RetrievedItem
    {
        public IEnumerable<InvertedIndex> Index { get; set; }
    }
}
