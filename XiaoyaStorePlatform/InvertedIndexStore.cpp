#include "stdafx.h"
#include "InvertedIndexStore.h"
#include "UrlHelper.h"
#include "DateTimeHelper.h"
#include "StoreException.h"
#include "IdListOperator.h"

using namespace rocksdb;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Exception;

const std::string InvertedIndexStore::DbName = "InvertedIndexStore";

const std::string InvertedIndexStore::MetaMaxIndexId = "MaxIndexId";

const std::string InvertedIndexStore::IndexKeyCFName = "index_key";
const size_t InvertedIndexStore::IndexKeyCF = 1;

const std::string InvertedIndexStore::UrlFileIdIndexCFName = "urlfile_id_index";
const size_t InvertedIndexStore::UrlFileIdIndexCF = 2;


bool InvertedIndexStore::GetIndex(const uint64_t id, Index & outIndex) const
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), SerializeHelper::SerializeUInt64(id), &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "InvertedIndexStore::GetIndex failed to get id: "
			+ std::to_string(id));
	}
	outIndex = SerializeHelper::Deserialize<Index>(data);
	return true;
}

bool InvertedIndexStore::GetIndexIds(const uint64_t urlFileId, IdList & outIdList) const
{
	auto key = SerializeHelper::SerializeUInt64(urlFileId);

	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[UrlFileIdIndexCF].get(), key, &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "InvertedIndexStore::GetIndexIds failed to get urlFileId: "
			+ std::to_string(urlFileId));
	}
	outIdList = SerializeHelper::Deserialize<IdList>(data);
	return true;
}

bool InvertedIndexStore::GetIndex(const IndexKey & indexKey, Index & outIndex) const
{
	auto key = SerializeHelper::Serialize(indexKey);

	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[IndexKeyCF].get(), key, &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status,
			"InvertedIndexStore::GetIndex failed to get IndexKey (urlFileId: "
			+ std::to_string(indexKey.urlfile_id()) + ", word: " + indexKey.word()
			+ ")");
	}
	auto id = SerializeHelper::DeserializeUInt64(data);

	return GetIndex(id, outIndex);
}

void InvertedIndexStore::ClearIndicesOf(const uint64_t urlFileId, WriteBatch &batch)
{
	IdList indexIdList;
	if (GetIndexIds(urlFileId, indexIdList))
	{
		for (auto indexId : indexIdList.ids())
		{
			Index index;
			if (GetIndex(indexId, index))
			{
				// Delete it
				batch.Delete(SerializeHelper::SerializeUInt64(indexId));

				// Delete IndexKey index
				auto key = SerializeHelper::Serialize(index.key());
				batch.Delete(mCFHandles[IndexKeyCF].get(), key);
			}
		}
	}
	// Delete UrlFileId index
	batch.Delete(mCFHandles[UrlFileIdIndexCF].get(),
		SerializeHelper::SerializeUInt64(urlFileId));
}

InvertedIndexStore::InvertedIndexStore(StoreConfig config, bool isReadOnly)
	: CounterBaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

void InvertedIndexStore::ClearAndSaveIndicesOf(const uint64_t urlFileId,
	const uint64_t oldUrlFileId, const std::vector<Index>& indices)
{
	WriteBatch batch;

	if (oldUrlFileId > 0)
	{
		ClearIndicesOf(oldUrlFileId, batch);
	}

	for (auto index : indices)
	{
		// Assign new id
		auto id = GetAndUpdateValue(MetaMaxIndexId, 1) + 1;
		index.set_index_id(id);

		// Save Index
		auto data = SerializeHelper::Serialize(index);
		batch.Put(SerializeHelper::SerializeUInt64(id), data);

		// Save UrlFileId index
		IdList deltaUrlFileIdIndex;
		deltaUrlFileIdIndex.set_is_add(true);
		deltaUrlFileIdIndex.add_ids(id);

		batch.Merge(mCFHandles[UrlFileIdIndexCF].get(),
			SerializeHelper::SerializeUInt64(urlFileId),
			SerializeHelper::Serialize(deltaUrlFileIdIndex));
		auto k = SerializeHelper::Serialize(index.key());
		// Save IndexKey index
		batch.Put(mCFHandles[IndexKeyCF].get(), 
			SerializeHelper::Serialize(index.key()),
			SerializeHelper::SerializeUInt64(id));
	}

	mDb->Write(WriteOptions(), &batch);
}

bool InvertedIndexStore::GetIndex(const uint64_t urlFileId,
	const std::string & word, Index & outIndex)
{
	IndexKey indexKey;
	indexKey.set_urlfile_id(urlFileId);
	indexKey.set_word(word);

	return GetIndex(indexKey, outIndex);
}

const std::vector<rocksdb::ColumnFamilyDescriptor>
InvertedIndexStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions idListOptions;
	idListOptions.merge_operator.reset(new MergeOperator::IdListOperator());
	idListOptions.max_successive_merges = 1000;

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, ColumnFamilyOptions()),
			ColumnFamilyDescriptor(IndexKeyCFName, idListOptions),
			ColumnFamilyDescriptor(UrlFileIdIndexCFName, idListOptions),
	});
}
