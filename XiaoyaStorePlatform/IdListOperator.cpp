#include "stdafx.h"
#include "IdListOperator.h"
#include "SerializeHelper.h"

using namespace rocksdb;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store::MergeOperator;
using namespace XiaoyaStore::Helper;

bool IdListOperator::FullMergeV2(const MergeOperationInput & merge_in,
	MergeOperationOutput * merge_out) const
{
	std::set<uint64_t> idSet;
	if (merge_in.existing_value != nullptr)
	{
		auto ids = SerializeHelper::Deserialize<IdList>(
			merge_in.existing_value->ToString()).ids();
		idSet.insert(ids.begin(), ids.end());
	}

	for (auto operand : merge_in.operand_list)
	{
		auto delta = SerializeHelper::Deserialize<IdList>(operand.ToString());
		if (delta.is_add())
		{
			idSet.insert(delta.ids().begin(), delta.ids().end());
		}
		else
		{
			std::set<uint64_t> result;
			std::set_difference(idSet.begin(), idSet.end(), delta.ids().begin(), delta.ids().end(),
				std::inserter(result, result.end()));
			idSet.swap(result);
		}
	}

	IdList result;
	result.set_is_add(true);
	*result.mutable_ids()
		= ::google::protobuf::RepeatedField<google::protobuf::uint64>(idSet.begin(), idSet.end());

	merge_out->new_value = SerializeHelper::Serialize(result);
	return true;
}

bool IdListOperator::PartialMerge(const Slice & key,
	const Slice & left_operand,
	const Slice & right_operand,
	std::string * new_value,
	Logger * logger) const
{
	auto leftDelta = SerializeHelper::Deserialize<IdList>(left_operand.ToString());
	auto rightDelta = SerializeHelper::Deserialize<IdList>(right_operand.ToString());

	if (leftDelta.is_add() == rightDelta.is_add())
	{
		leftDelta.mutable_ids()->MergeFrom(rightDelta.ids());
		*new_value = SerializeHelper::Serialize(leftDelta);
		return true;
	}

	return false;
}
