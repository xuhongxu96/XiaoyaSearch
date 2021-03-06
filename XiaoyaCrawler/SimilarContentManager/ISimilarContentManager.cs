﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCrawler.Fetcher;

namespace XiaoyaCrawler.SimilarContentManager
{
    /// <summary>
    /// Similiar content manager
    /// To judge if two web content is similiar and group their urls
    /// </summary>
    public interface ISimilarContentManager
    {
        (string Url, string Content) JudgeContent(FetchedFile fetchedFile, string content);
    }
}
