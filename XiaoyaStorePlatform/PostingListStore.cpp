#include "stdafx.h"
#include "PostingListStore.h"
#include "UrlHelper.h"
#include "DateTimeHelper.h"
#include "StoreException.h"
#include "PostingListOperator.h"

using namespace rocksdb;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Exception;

PostingListStore::PostingListStore(StoreConfig config, bool isReadOnly)
	: BaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

void PostingListStore::SavePostingList(PostingList & deltaPostingList)
{
	auto data = SerializeHelper::Serialize(deltaPostingList);
	mDb->Merge(WriteOptions(), deltaPostingList.Word, data);
}

bool PostingListStore::LoadPostingList(std::string & word, PostingList & outPostingList)
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), word, &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "PostingListStore::LoadPostingList failed to load: " + word);
	}
	outPostingList = SerializeHelper::Deserialize<PostingList>(data);
	return true;
}

const std::vector<ColumnFamilyDescriptor>
PostingListStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions options;
	options.merge_operator.reset(new MergeOperator::PostingListOperator());

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, options),
	});
}
