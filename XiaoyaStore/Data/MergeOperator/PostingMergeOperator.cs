using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Data.MergeOperator
{
    public sealed class PostingMergeOperator : MergeOperatorBase
    {
        public PostingMergeOperator() : base("Posting Merge") { }

        protected override byte[] OnFullMerge(byte[] key, byte[] existingValue, byte[][] operands, out bool success)
        {
            PostingList postingList;
            if (existingValue == null)
            {
                postingList = new PostingList();
            }
            else
            {
                postingList = ModelSerializer.DeserializeModel<PostingList>(existingValue);
            }

            foreach (var operand in operands)
            {
                var nextList = ModelSerializer.DeserializeModel<PostingList>(operand);
                if (nextList.IsAdd)
                {
                    postingList.Postings.UnionWith(nextList.Postings);
                }
                else
                {
                    postingList.Postings.ExceptWith(nextList.Postings);
                }

                postingList.WordFrequency += nextList.WordFrequency;
                postingList.DocumentFrequency += nextList.DocumentFrequency;
            }

            if (postingList.WordFrequency < 0)
            {
                postingList.WordFrequency = 0;
            }

            if (postingList.DocumentFrequency < 0)
            {
                postingList.DocumentFrequency = 0;
            }

            success = true;
            return ModelSerializer.SerializeModel(postingList);
        }

        protected override byte[] OnPartialMerge(byte[] key, byte[][] operands, out bool success)
        {
            PostingList postingList = new PostingList();

            foreach (var operand in operands)
            {
                var nextList = ModelSerializer.DeserializeModel<PostingList>(operand);
                if (nextList.IsAdd)
                {
                    postingList.Postings.UnionWith(nextList.Postings);
                }
                else
                {
                    postingList.Postings.ExceptWith(nextList.Postings);
                }

                postingList.WordFrequency += nextList.WordFrequency;
                postingList.DocumentFrequency += nextList.DocumentFrequency;
            }

            if (postingList.WordFrequency < 0)
            {
                postingList.WordFrequency = 0;
            }

            if (postingList.DocumentFrequency < 0)
            {
                postingList.DocumentFrequency = 0;
            }

            success = true;
            return ModelSerializer.SerializeModel(postingList);
        }
    }
}
