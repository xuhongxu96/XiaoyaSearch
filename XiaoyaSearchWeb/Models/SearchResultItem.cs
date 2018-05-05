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

        public DateTime PublishDate { get; set; }

        public double Score { get; set; }
        public string ScoreDebugInfo { get; set; }
        public double ProScore { get; set; }
        public string ProScoreDebugInfo { get; set; }
    }
}
