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
	auto ids = merge_in.existing_value == nullptr ?
		std::set<uint64_t>() 
		: SerializeHelper::Deserialize<IdList>(merge_in.existing_value->ToString()).Ids;

	for (auto operand : merge_in.operand_list)
	{
		auto delta = SerializeHelper::Deserialize<IdList>(operand.ToString());
		if (delta.IsAdd)
		{
			ids.insert(delta.Ids.begin(), delta.Ids.end());
		}
		else
		{
			std::set<uint64_t> result;
			std::set_difference(ids.begin(), ids.end(), delta.Ids.begin(), delta.Ids.end(),
				std::inserter(result, result.end()));
			ids.swap(result);
		}
	}

	IdList result;
	result.IsAdd = true;
	result.Ids = ids;

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

	if (leftDelta.IsAdd == rightDelta.IsAdd)
	{
		leftDelta.Ids.insert(rightDelta.Ids.begin(), rightDelta.Ids.end());
		*new_value = SerializeHelper::Serialize(leftDelta);
		return true;
	}

	return false;
}
