using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Data.MergeOperator
{
    public sealed class CounterOperator : MergeOperatorBase
    {
        public CounterOperator() : base("Counter") { }

        protected override byte[] OnFullMerge(byte[] key, byte[] existingValue, byte[][] operands, out bool success)
        {
            long result = 0;
            if (existingValue != null)
            {
                result = BitConverter.ToInt64(existingValue, 0);
            }

            foreach (var operand in operands)
            {
                result += BitConverter.ToInt64(operand, 0);
            }
            if (result < 0) result = 0;

            success = true;
            return result.GetBytes();
        }

        protected override byte[] OnPartialMerge(byte[] key, byte[][] operands, out bool success)
        {
            long result = 0;

            foreach (var operand in operands)
            {
                result += BitConverter.ToInt64(operand, 0);
            }
            if (result < 0) result = 0;

            success = true;
            return result.GetBytes();
        }
    }
}
