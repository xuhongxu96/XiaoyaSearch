#include "stdafx.h"
#include "PostingListOperator.h"
#include "SerializeHelper.h"

using namespace rocksdb;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store::MergeOperator;
using namespace XiaoyaStore::Helper;

bool PostingListOperator::FullMergeV2(const MergeOperationInput & merge_in,
	MergeOperationOutput * merge_out) const
{
	auto postingList = merge_in.existing_value == nullptr ?
		PostingList()
		: SerializeHelper::Deserialize<PostingList>(merge_in.existing_value->ToString());

	for (auto operand : merge_in.operand_list)
	{
		auto delta = SerializeHelper::Deserialize<PostingList>(operand.ToString());
		if (delta.IsAdd)
		{
			postingList.Postings.insert(delta.Postings.begin(), delta.Postings.end());
		}
		else
		{
			std::set<uint64_t> result;
			std::set_difference(postingList.Postings.begin(), 
				postingList.Postings.end(), 
				delta.Postings.begin(),
				delta.Postings.end(),
				std::inserter(result, result.end()));
			postingList.Postings.swap(result);
		}
		postingList.WordFrequency += delta.WordFrequency;
		postingList.DocumentFrequency += delta.DocumentFrequency;
	}

	if (postingList.WordFrequency < 0)
	{
		postingList.WordFrequency = 0;
	}

	if (postingList.DocumentFrequency < 0)
	{
		postingList.DocumentFrequency = 0;
	}

	merge_out->new_value = SerializeHelper::Serialize(postingList);
	return true;
}

bool PostingListOperator::PartialMerge(const Slice & key,
	const Slice & left_operand,
	const Slice & right_operand,
	std::string * new_value,
	Logger * logger) const
{
	auto leftDelta = SerializeHelper::Deserialize<PostingList>(left_operand.ToString());
	auto rightDelta = SerializeHelper::Deserialize<PostingList>(right_operand.ToString());

	if (leftDelta.IsAdd == rightDelta.IsAdd)
	{
		leftDelta.Postings.insert(rightDelta.Postings.begin(), rightDelta.Postings.end());
		leftDelta.WordFrequency += rightDelta.WordFrequency;
		leftDelta.DocumentFrequency += rightDelta.DocumentFrequency;
		*new_value = SerializeHelper::Serialize(leftDelta);
		return true;
	}
	return false;
}
