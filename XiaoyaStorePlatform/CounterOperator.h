#pragma once

#include "stdafx.h"

using namespace rocksdb;

namespace XiaoyaStore
{
	namespace Store
	{
		namespace MergeOperator
		{
			class CounterOperator : public MergeOperator
			{
				// Inherited via MergeOperator
				virtual const char * Name() const override
				{
					return "CounterOperator";
				}

				virtual bool FullMergeV2(const MergeOperationInput& merge_in,
					MergeOperationOutput* merge_out) const override;

				virtual bool PartialMerge(const Slice& key, const Slice& left_operand,
					const Slice& right_operand, std::string* new_value,
					Logger* logger) const override;
			};
		}
	}
}