using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Store;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class Word : Expression
    {
        public string Value { get; private set; }

        protected long mFrequency;
        public override long Frequency => mFrequency;

        public override bool IsIncluded => true;

        protected RetrieverConfig mConfig;

        public Word(string value)
        {
            Value = value;
        }

        public override IEnumerable<RetrievedUrlFilePositions> Retrieve()
        {
            var indices = mConfig.InvertedIndexStore.LoadByWord(Value);
            return from index in indices
                   group new WordPosition
                   {
                       Word = index.Word,
                       Position = index.Position,
                   } by index.UrlFileId into g
                   select new RetrievedUrlFilePositions(g.Key, g);
        }

        public override void SetConfig(RetrieverConfig config)
        {
            var stat = config.IndexStatStore.LoadByWord(Value);
            if (stat == null)
            {
                mFrequency = 0;
            }
            else
            {
                mFrequency = stat.WordFrequency;
            }
            mConfig = config;
        }
    }
}
