using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Data.MergeOperator
{
    public sealed class IdListConcatOperator : MergeOperatorBase
    {
        public IdListConcatOperator() : base("List Concat") { }

        protected override byte[] OnFullMerge(byte[] key, byte[] existingValue, byte[][] operands, out bool success)
        {
            HashSet<long> ids;
            if (existingValue == null)
            {
                ids = new HashSet<long>();
            }
            else
            {
                ids = ModelSerializer.DeserializeModel<IdList>(existingValue).Ids;
            }

            foreach (var operand in operands)
            {
                var nextList = ModelSerializer.DeserializeModel<IdList>(operand);
                if (nextList.IsAdd)
                {
                    ids.UnionWith(nextList.Ids);
                }
                else
                {
                    ids.ExceptWith(nextList.Ids);
                }
            }

            var result = new IdList
            {
                Ids = ids,
            };

            success = true;
            return ModelSerializer.SerializeModel(result);
        }

        protected override byte[] OnPartialMerge(byte[] key, byte[][] operands, out bool success)
        {
            var ids = new HashSet<long>();

            foreach (var operand in operands)
            {
                var nextList = ModelSerializer.DeserializeModel<IdList>(operand);
                if (nextList.IsAdd)
                {
                    ids.UnionWith(nextList.Ids);
                }
                else
                {
                    ids.ExceptWith(nextList.Ids);
                }
            }

            var result = new IdList
            {
                Ids = ids,
            };

            success = true;
            return ModelSerializer.SerializeModel(result);
        }
    }
}
