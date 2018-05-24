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
	std::set<uint64_t> postingSet;
	uint64_t wordFrequency = 0, documentFrequency = 0;
	std::string word;
	if (merge_in.existing_value != nullptr)
	{
		auto postingList = SerializeHelper::Deserialize<PostingList>(
			merge_in.existing_value->ToString());
		postingSet.insert(postingList.postings().begin(), postingList.postings().end());
		wordFrequency = postingList.word_frequency();
		documentFrequency = postingList.document_frequency();
		word = postingList.word();
	}

	for (auto operand : merge_in.operand_list)
	{
		auto delta = SerializeHelper::Deserialize<PostingList>(operand.ToString());
		if (delta.is_add())
		{
			postingSet.insert(delta.postings().begin(), delta.postings().end());

			wordFrequency += delta.word_frequency();
			documentFrequency += delta.document_frequency();
		}
		else
		{
			std::set<uint64_t> result;
			std::set_difference(postingSet.begin(),
				postingSet.end(),
				delta.postings().begin(),
				delta.postings().end(),
				std::inserter(result, result.end()));
			postingSet.swap(result);

			wordFrequency -= delta.word_frequency();
			documentFrequency -= delta.document_frequency();
		}
		if (word.empty())
		{
			word = delta.word();
		}
	}

	if (wordFrequency < 0)
	{
		wordFrequency = 0;
	}

	if (documentFrequency < 0)
	{
		documentFrequency = 0;
	}

	PostingList result;
	result.set_is_add(true);
	*result.mutable_postings()
		= ::google::protobuf::RepeatedField<google::protobuf::uint64>(
			postingSet.begin(), postingSet.end());
	result.set_word_frequency(wordFrequency);
	result.set_document_frequency(documentFrequency);
	result.set_word(word);

	merge_out->new_value = SerializeHelper::Serialize(result);
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

	if (leftDelta.is_add() == rightDelta.is_add())
	{
		leftDelta.mutable_postings()->MergeFrom(rightDelta.postings());
		leftDelta.set_word_frequency(leftDelta.word_frequency() + rightDelta.word_frequency());
		leftDelta.set_document_frequency(leftDelta.document_frequency() + rightDelta.document_frequency());
		*new_value = SerializeHelper::Serialize(leftDelta);
		return true;
	}
	return false;
}
