using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoyaSearchWeb.Models
{
    public class WebSearchResult
    {
        public string Query { get; set; }
        public List<SearchResultItem> Items = new List<SearchResultItem>();
    }
}
