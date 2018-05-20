#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Store
	{
		namespace MergeOperator
		{
			class IdListOperator : public rocksdb::MergeOperator
			{
				// Inherited via MergeOperator
				virtual const char * Name() const override
				{
					return "IdListOperator";
				}

				virtual bool FullMergeV2(const MergeOperationInput& merge_in,
					MergeOperationOutput* merge_out) const override;

				virtual bool PartialMerge(const rocksdb::Slice& key, const rocksdb::Slice& left_operand,
					const rocksdb::Slice& right_operand, std::string* new_value,
					rocksdb::Logger* logger) const override;
			};
		}
	}
}