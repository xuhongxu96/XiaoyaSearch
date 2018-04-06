using XiaoyaRetriever.Expression;

namespace XiaoyaQueryParser.QueryParser
{
    public interface IQueryParser
    {
        ParsedQuery Parse(string query);
    }
}