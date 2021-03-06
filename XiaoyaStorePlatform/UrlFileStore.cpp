#include "stdafx.h"
#include "UrlFileStore.h"
#include "DateTimeHelper.h"
#include "StoreException.h"
#include "IdListOperator.h"
#include "CounterOperator.h"

using namespace rocksdb;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Exception;

const std::string UrlFileStore::DbName = "UrlFileStore";

const std::string UrlFileStore::MetaMaxUrlFileId = "MaxUrlFileId";

const std::string UrlFileStore::UrlIndexCFName = "url_index";
const size_t UrlFileStore::UrlIndexCF = 1;

const std::string UrlFileStore::HashIndexCFName = "hash_index";
const size_t UrlFileStore::HashIndexCF = 2;

uint64_t UrlFileStore::GetMaxUrlFileId() const
{
	return GetValue(MetaMaxUrlFileId);
}

bool UrlFileStore::GetUrlFileIdListByHash(const std::string &hash, IdList &outIdList) const
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[HashIndexCF].get(), hash, &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status,
			"UrlFileStore::GetUrlFileIdListByHash failed to get hash: " + hash);
	}
	outIdList = SerializeHelper::Deserialize<IdList>(data);
	return true;
}

UrlFileStore::UrlFileStore(StoreConfig config, bool isReadOnly)
	: CounterBaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

bool UrlFileStore::GetUrlFile(const uint64_t urlFileId, UrlFile &outUrlFile) const
{
	auto key = SerializeHelper::SerializeUInt64(urlFileId);
	std::string data;
	auto status = mDb->Get(ReadOptions(), key, &data);

	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status,
			"UrlFileStore::GetUrlFile failed to get id of: " + std::to_string(urlFileId));
	}

	outUrlFile = SerializeHelper::Deserialize<UrlFile>(data);
	return true;
}

bool UrlFileStore::GetUrlFile(const std::string &url, UrlFile &outUrlFile) const
{
	std::string key, data;

	auto status = mDb->Get(ReadOptions(), mCFHandles[UrlIndexCF].get(), url, &key);

	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "UrlFileStore::GetUrlFile failed to get url of: " + url);
	}

	status = mDb->Get(ReadOptions(), key, &data);

	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "UrlFileStore::GetUrlFile failed to get url of: " + url);
	}

	outUrlFile = SerializeHelper::Deserialize<UrlFile>(data);
	return true;
}

std::vector<UrlFile> UrlFileStore::GetUrlFilesByHash(const std::string & hash)
{
	std::vector<UrlFile> result;
	IdList idList;
	if (GetUrlFileIdListByHash(hash, idList))
	{
		result.reserve(idList.ids_size());

		for (auto id : idList.ids())
		{
			UrlFile urlFile;
			if (GetUrlFile(id, urlFile))
			{
				result.push_back(urlFile);
			}
		}
	}
	return result;
}

uint64_t UrlFileStore::SaveUrlFileAndGetOldId(UrlFile & urlFile)
{
	uint64_t oldUrlFileId = 0;

	WriteBatch batch;
	bool willAddToIndexQueue = false;
	auto now = DateTimeHelper::Now();

	UrlFile oldUrlFile;
	if (GetUrlFile(urlFile.url(), oldUrlFile))
	{
		// Exists old UrlFile with the same Url
		oldUrlFileId = oldUrlFile.url_file_id();

		urlFile.set_created_at(oldUrlFile.created_at());

		auto updateInterval = now - oldUrlFile.updated_at();
		updateInterval = static_cast<uint64_t>((oldUrlFile.update_interval() * 3.0
			+ updateInterval) / 4.0);

		if (updateInterval < DateTimeHelper::FromHours(6))
		{
			updateInterval = DateTimeHelper::FromHours(6);
		}
		urlFile.set_update_interval(updateInterval);


		if (oldUrlFile.title() != urlFile.title()
			|| oldUrlFile.content() != urlFile.content())
		{
			// Title or Content changed
			urlFile.set_updated_at(now);
			willAddToIndexQueue = true;
		}
		else
		{
			urlFile.set_updated_at(oldUrlFile.updated_at());
		}

		// Remove old hash index
		IdList removingHashIndex;
		removingHashIndex.set_is_add(false);
		removingHashIndex.add_ids(oldUrlFile.url_file_id());

		batch.Merge(mCFHandles[HashIndexCF].get(), oldUrlFile.file_hash(),
			SerializeHelper::Serialize(removingHashIndex));

		// Delete old UrlFile
		batch.Delete(SerializeHelper::SerializeUInt64(oldUrlFile.url_file_id()));
	}
	else
	{
		// New Url
		oldUrlFileId = 0;

		urlFile.set_updated_at(now);
		urlFile.set_created_at(now);
		urlFile.set_update_interval(DateTimeHelper::FromDays(1));

		willAddToIndexQueue = true;
	}

	// Assign new id
	auto id = GetAndUpdateValue(MetaMaxUrlFileId, 1) + 1;
	urlFile.set_url_file_id(id);
	auto idData = SerializeHelper::SerializeUInt64(id);

	// Overwrite url index
	batch.Put(mCFHandles[UrlIndexCF].get(), urlFile.url(), idData);

	// Add new hash index
	IdList newHashIndex;
	newHashIndex.set_is_add(true);
	newHashIndex.add_ids(id);

	batch.Merge(mCFHandles[HashIndexCF].get(), urlFile.file_hash(),
		SerializeHelper::Serialize(newHashIndex));

	// Add new UrlFile
	batch.Put(idData, SerializeHelper::Serialize(urlFile));

	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status,
			"UrlFileStore::SaveUrlFileAndGetOldId failed to save: " + urlFile.url());
	}

	return oldUrlFileId;
}

uint64_t UrlFileStore::GetCount() const
{
	return GetMaxUrlFileId();
}

bool UrlFileStore::ContainsId(const uint64_t id)
{
	auto status = mDb->Get(ReadOptions(), SerializeHelper::SerializeUInt64(id), &std::string());
	if (!status.ok())
	{
		throw StoreException(status,
			"UrlFileStore::ContainId failed to get: " + std::to_string(id));
	}
	return !status.IsNotFound();
}

const std::vector<rocksdb::ColumnFamilyDescriptor>
UrlFileStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions idListOptions;
	idListOptions.merge_operator.reset(new MergeOperator::IdListOperator());
	idListOptions.max_successive_merges = 1000;

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, ColumnFamilyOptions()),
			ColumnFamilyDescriptor(UrlIndexCFName, idListOptions),
			ColumnFamilyDescriptor(HashIndexCFName, idListOptions),
	});
}
