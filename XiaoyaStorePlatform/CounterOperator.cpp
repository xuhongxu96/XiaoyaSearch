#include "stdafx.h"
#include "CounterOperator.h"
#include "SerializerHelper.h"

using namespace XiaoyaStore::Store::MergeOperator;
using namespace XiaoyaStore::Helper;

bool CounterOperator::FullMergeV2(const MergeOperationInput & merge_in,
	MergeOperationOutput * merge_out) const
{
	auto count = merge_in.existing_value == nullptr ?
		0 : SerializerHelper::DeserializeUInt64(merge_in.existing_value->ToString());

	for (auto operand : merge_in.operand_list)
	{
		auto delta = SerializerHelper::DeserializeInt64(operand.ToString());
		count += delta;
	}

	merge_out->new_value = SerializerHelper::SerializeUInt64(count);
	return true;
}

bool CounterOperator::PartialMerge(const Slice & key,
	const Slice & left_operand,
	const Slice & right_operand,
	std::string * new_value,
	Logger * logger) const
{
	auto leftDelta = SerializerHelper::DeserializeInt64(left_operand.ToString());
	auto rightDelta = SerializerHelper::DeserializeInt64(right_operand.ToString());

	*new_value = SerializerHelper::SerializeInt64(leftDelta + rightDelta);
	return true;
}
