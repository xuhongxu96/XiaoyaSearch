#include "stdafx.h"
#include "PostingListStore.h"
#include "UrlHelper.h"
#include "DateTimeHelper.h"
#include "StoreException.h"
#include "PostingListOperator.h"
#include "GenericSetOperator.h"

using namespace rocksdb;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Exception;

const std::string PostingListStore::DbName = "PostingListStore";

const std::string PostingListStore::UrlFileIdIndexCFName = "url_file_id_index";
const size_t PostingListStore::UrlFileIdIndexCF = 1;

PostingListStore::PostingListStore(StoreConfig config, bool isReadOnly)
	: BaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

void PostingListStore::SavePostingLists(const uint64_t urlFileId,
	const std::vector<PostingList> & deltaPostingLists)
{
	WriteBatch batch;
	PostingLists postingLists;

	for (auto deltaPostingList : deltaPostingLists)
	{
		auto data = SerializeHelper::Serialize(deltaPostingList);
		batch.Merge(deltaPostingList.word(), data);

		*postingLists.add_items() = deltaPostingList;
	}
	// Save UrlFileId Index
	batch.Put(mCFHandles[UrlFileIdIndexCF].get(),
		SerializeHelper::SerializeUInt64(urlFileId),
		SerializeHelper::Serialize(postingLists));

	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "PostingListStore::SavePostingList failed to save: "
			+ std::to_string(urlFileId));
	}
}

void PostingListStore::ClearPostingLists(const uint64_t urlFileId)
{
	auto key = SerializeHelper::SerializeUInt64(urlFileId);

	std::string data;
	auto status = mDb->Get(ReadOptions(),
		mCFHandles[UrlFileIdIndexCF].get(),
		key,
		&data);
	if (status.IsNotFound())
	{
		return;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "PostingListStore::ClearPostingLists failed to get posting lists for "
			+ std::to_string(urlFileId));
	}

	auto postingLists = SerializeHelper::Deserialize<PostingLists>(data);

	WriteBatch batch;

	for (auto postingList : postingLists.items())
	{
		postingList.set_is_add(false);

		batch.Merge(postingList.word(), SerializeHelper::Serialize(postingList));
	}
	batch.Delete(mCFHandles[UrlFileIdIndexCF].get(), key);

	status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "PostingListStore::ClearPostingLists failed to clear posting lists for "
			+ std::to_string(urlFileId));
	}
}

bool PostingListStore::GetPostingList(const std::string &word, PostingList & outPostingList) const
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), word, &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "PostingListStore::GetPostingList failed to load: " + word);
	}
	outPostingList = SerializeHelper::Deserialize<PostingList>(data);
	return true;
}

const std::vector<ColumnFamilyDescriptor>
PostingListStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions options;
	options.max_successive_merges = 1000;
	options.merge_operator.reset(new MergeOperator::PostingListOperator());

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, options),
			ColumnFamilyDescriptor(UrlFileIdIndexCFName, ColumnFamilyOptions()),
	});
}
