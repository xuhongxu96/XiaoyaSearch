#include "stdafx.h"
#include "CounterOperator.h"
#include "SerializeHelper.h"

using namespace rocksdb;
using namespace XiaoyaStore::Store::MergeOperator;
using namespace XiaoyaStore::Helper;

bool CounterOperator::FullMergeV2(const MergeOperationInput & merge_in,
	MergeOperationOutput * merge_out) const
{
	auto count = merge_in.existing_value == nullptr ?
		0 : SerializeHelper::DeserializeUInt64(merge_in.existing_value->ToString());

	for (auto operand : merge_in.operand_list)
	{
		auto delta = SerializeHelper::DeserializeInt64(operand.ToString());
		count += delta;
	}

	merge_out->new_value = SerializeHelper::SerializeUInt64(count);
	return true;
}

bool CounterOperator::PartialMerge(const Slice & key,
	const Slice & left_operand,
	const Slice & right_operand,
	std::string * new_value,
	Logger * logger) const
{
	auto leftDelta = SerializeHelper::DeserializeInt64(left_operand.ToString());
	auto rightDelta = SerializeHelper::DeserializeInt64(right_operand.ToString());

	*new_value = SerializeHelper::SerializeInt64(leftDelta + rightDelta);
	return true;
}
