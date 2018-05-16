using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaStore.Data.Model;

namespace XiaoyaFileParser
{
    public interface IFileParser
    {
        void SetFile(string mimeType, string url, string charset, string filePath = null);
        Task<IList<string>> GetUrlsAsync();
        Task<IList<LinkInfo>> GetLinksAsync();
        Task<IList<Header>> GetHeadersAsync();
        Task<IList<Token>> GetTokensAsync();
        Task<IList<Token>> GetTokensAsync(IEnumerable<string> linkTexts);
        Task<string> GetTitleAsync();
        Task<string> GetContentAsync();
        Task<string> GetTextContentAsync();
        Task<DateTime> GetPublishDateAsync();
    }
}