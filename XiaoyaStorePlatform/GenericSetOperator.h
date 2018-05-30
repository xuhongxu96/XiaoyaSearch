#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Store
	{
		namespace MergeOperator
		{
			template <typename TSet, typename TItem, typename CompareItem>
			class GenericSetOperator : public rocksdb::MergeOperator
			{
				// Inherited via MergeOperator
				virtual const char * Name() const override
				{
					return "GenericSetOperator";
				}

				virtual bool FullMergeV2(const MergeOperationInput& merge_in,
					MergeOperationOutput* merge_out) const override
				{
					std::set<TItem, CompareItem> itemSet;
					if (merge_in.existing_value != nullptr)
					{
						auto items = SerializeHelper::Deserialize<TSet>(
							merge_in.existing_value->ToString()).items();
						itemSet.insert(items.begin(), items.end());
					}

					for (auto operand : merge_in.operand_list)
					{
						auto delta = SerializeHelper::Deserialize<TSet>(operand.ToString());
						if (delta.is_add())
						{
							itemSet.insert(delta.items().begin(), delta.items().end());
						}
						else
						{
							for (auto item : delta.items())
							{
								itemSet.erase(item);
							}
						}
					}

					TSet result;
					result.set_is_add(true);
					*result.mutable_items()
						= ::google::protobuf::RepeatedPtrField<TItem>(itemSet.begin(), itemSet.end());

					merge_out->new_value = SerializeHelper::Serialize(result);
					return true;
				}

				virtual bool PartialMerge(const rocksdb::Slice& key, const rocksdb::Slice& left_operand,
					const rocksdb::Slice& right_operand, std::string* new_value,
					rocksdb::Logger* logger) const override
				{
					auto leftDelta = SerializeHelper::Deserialize<TSet>(left_operand.ToString());
					auto rightDelta = SerializeHelper::Deserialize<TSet>(right_operand.ToString());

					if (leftDelta.is_add() == rightDelta.is_add())
					{
						leftDelta.mutable_items()->MergeFrom(rightDelta.items());
						*new_value = SerializeHelper::Serialize(leftDelta);
						return true;
					}

					return false;
				}
			};
		}
	}
}
