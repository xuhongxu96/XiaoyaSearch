using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoyaSearchWeb.Models
{
    public class SearchResultItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Details { get; set; }

        public double Score { get; set; }
        public double ProScore { get; set; }
    }
}
