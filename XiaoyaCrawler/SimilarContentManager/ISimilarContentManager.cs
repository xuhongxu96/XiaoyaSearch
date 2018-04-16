using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCrawler.Parser;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.SimilarContentManager
{
    /// <summary>
    /// Similiar content manager
    /// To judge if two web content is similiar and group their urls
    /// </summary>
    public interface ISimilarContentManager
    {
        /// <summary>
        /// Judge if the content has been seen before
        /// </summary>
        /// <param name="urlFile">Fetched UrlFile</param>
        /// <returns>The UrlFile that has same content. If no same file, retunrs null</returns>
        UrlFile JudgeContent(UrlFile urlFile);
    }
}
