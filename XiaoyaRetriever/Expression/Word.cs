﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaCommon.Helper;
using XiaoyaRetriever.Config;
using XiaoyaStore.Store;

namespace XiaoyaRetriever.Expression
{
    public class Word : SearchExpression
    {
        public string Value { get; private set; }

        protected ulong mFrequency = 1;
        public override ulong DocumentFrequency => mFrequency;
        protected ulong mWordFrequency = 1;
        public ulong WordFrequency => mWordFrequency;

        public override bool IsIncluded => true;

        protected RetrieverConfig mConfig;

        public Word(string value)
        {
            Value = value;
        }

        public override void SetConfig(RetrieverConfig config)
        {
            var stat = config.PostingListStore.GetPostingList(Value);
            if (stat != null)
            {
                mFrequency = stat.DocumentFrequency;
                mWordFrequency = stat.WordFrequency;
            }
            mConfig = config;
        }

        public override bool Equals(object obj)
        {
            var word = obj as Word;
            return word != null &&
                   Value == word.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
    }
}
