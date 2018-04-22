using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaQueryParser.Config;
using XiaoyaRetriever.Expression;

namespace XiaoyaQueryParser.QueryParser
{
    public class SimpleQueryParser : IQueryParser
    {
        protected QueryParserConfig mConfig = new QueryParserConfig();

        public SimpleQueryParser()
        { }

        public SimpleQueryParser(QueryParserConfig config)
        {
            mConfig = config;
        }

        public ParsedQuery Parse(string query)
        {
            var words = new List<string>();
            var result = new And();
            var rawTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawTerm in rawTerms)
            {
                var currentTerm = rawTerm.Trim();

                if (rawTerm.StartsWith("-"))
                {
                    // negate
                    currentTerm = rawTerm.Substring(1);

                    var segments = mConfig.TextSegmenter.Segment(currentTerm);
                    var subAnd = new And();

                    foreach (var segment in segments)
                    {
                        subAnd.Add(segment.Word);
                    }

                    result.Add(new Not(subAnd));
                }
                else
                {
                    var segments = mConfig.TextSegmenter.Segment(currentTerm);

                    foreach (var segment in segments)
                    {
                        result.Add(segment.Word);
                        words.Add(segment.Word);
                    }
                }
            }

            return new ParsedQuery
            {
                Expression = result,
                Words = words,
            };
        }
    }
}
