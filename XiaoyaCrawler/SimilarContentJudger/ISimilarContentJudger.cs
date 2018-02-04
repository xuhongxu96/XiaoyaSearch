using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCrawler.Parser;

namespace XiaoyaCrawler.SimilarContentJudger
{
    /// <summary>
    /// Similiar content judger
    /// To judge if two web content is similiar and group their urls
    /// </summary>
    public interface ISimilarContentJudger
    {
        /// <summary>
        /// Judge if the content has been seen before
        /// </summary>
        /// <param name="url">Web url</param>
        /// <param name="content">Web content to be judged</param>
        void AddContentAsync(string url, string content);
        /// <summary>
        /// Load check point to recover progress
        /// </summary>
        Task LoadCheckPoint();
        /// <summary>
        /// Save check point to recover progress
        /// </summary>
        Task SaveCheckPoint();
    }
}
