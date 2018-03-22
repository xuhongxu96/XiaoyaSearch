using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRetriever.BooleanRetriever.Expression;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever
{
    public class BooleanRetriever
    {

        protected RetrieverConfig mConfig;

        public BooleanRetriever(RetrieverConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<RetrievedUrlFilePositions> Retrieve(Expression.Expression expression)
        {
            expression.SetConfig(mConfig);

            if (expression.IsIncluded)
            {
                return expression.Retrieve();
            }
            else
            {
                throw new NotSupportedException("Infinite results to retrieve " +
                    "(Maybe you use NOT to get the complementary set of a finite result)");
            }
        }
    }
} 
