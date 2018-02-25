using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaStore.Data.Model;

namespace XiaoyaFileParser
{
    public interface IFileParser
    {
        UrlFile UrlFile { get; set; }
        Task<IList<string>> GetUrlsAsync();
        Task<IList<Token>> GetTokensAsync();
        Task<string> GetContentAsync();
        Task<string> GetTextContentAsync();
    }
}