using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Store;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class Word : IExpression
    {
        public string Value { get; private set; }
        public long Frequency { get; private set; }
        public bool IsIncluded => true;

        protected RetrieverConfig mConfig;

        public Word(string value, RetrieverConfig config)
        {
            Value = value;
            Frequency = config.IndexStatStore.LoadByWord(value).Count;

            mConfig = config;
        }

        public IEnumerable<InvertedIndex> Retrieve()
        {
            return from index in mConfig.InvertedIndexStore.LoadByWord(Value)
                   select index;
        }
    }
}
