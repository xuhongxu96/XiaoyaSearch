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

const std::string InvertedIndexStore::UrlFileIdIndexCFName = "url_file_id_index";
const size_t InvertedIndexStore::UrlFileIdIndexCF = 1;

InvertedIndexStore::InvertedIndexStore(StoreConfig config, bool isReadOnly)
	: BaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

void InvertedIndexStore::ClearIndices(const uint64_t urlFileId)
{
	auto urlFileIdData = SerializeHelper::SerializeUInt64(urlFileId);

	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[UrlFileIdIndexCF].get(),
		urlFileIdData, &data);

	if (status.IsNotFound())
	{
		return;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "InvertedIndexStore::ClearIndices failed to clear urlFile: "
			+ std::to_string(urlFileId));
	}

	auto keys = SerializeHelper::Deserialize<IndexKeys>(data);

	WriteBatch batch;

	for (auto key : keys.items())
	{
		batch.Delete(SerializeHelper::Serialize(key));
	}
	batch.Delete(mCFHandles[UrlFileIdIndexCF].get(), urlFileIdData);

	mDb->Write(WriteOptions(), &batch);
}

void InvertedIndexStore::SaveIndices(const uint64_t urlFileId,
	const std::vector<Index>& indices)
{
	WriteBatch batch;

	IndexKeys keys;

	for (auto index : indices)
	{
		// Save Index
		batch.Put(SerializeHelper::Serialize(index.key()),
			SerializeHelper::Serialize(index));
		// Add Keys
		*keys.add_items() = index.key();
	}

	// Save UrlFileId Index
	batch.Put(mCFHandles[UrlFileIdIndexCF].get(),
		SerializeHelper::SerializeUInt64(urlFileId),
		SerializeHelper::Serialize(keys));

	mDb->Write(WriteOptions(), &batch);
}

bool InvertedIndexStore::GetIndex(const uint64_t urlFileId,
	const std::string & word, Index & outIndex)
{
	IndexKey indexKey;
	indexKey.set_url_file_id(urlFileId);
	indexKey.set_word(word);

	std::string data;

	auto status = mDb->Get(ReadOptions(),
		SerializeHelper::Serialize(indexKey),
		&data);

	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "InvertedIndexStore::GetIndex failed to get index for: (urlFileId = "
			+ std::to_string(urlFileId) + ", word = " + word);
	}

	outIndex = SerializeHelper::Deserialize<Index>(data);
	return true;
}

const std::vector<rocksdb::ColumnFamilyDescriptor>
InvertedIndexStore::GetColumnFamilyDescriptors()
{
	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, ColumnFamilyOptions()),
			ColumnFamilyDescriptor(UrlFileIdIndexCFName, ColumnFamilyOptions()),
	});
}
