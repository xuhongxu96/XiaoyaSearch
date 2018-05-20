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

const std::string UrlFileStore::IndexQueueCFName = "index_queue";
const size_t UrlFileStore::IndexQueueCF = 1;

const std::string UrlFileStore::UrlIndexCFName = "url_index";
const size_t UrlFileStore::UrlIndexCF = 2;

const std::string UrlFileStore::HashIndexCFName = "hash_index";
const size_t UrlFileStore::HashIndexCF = 3;

void XiaoyaStore::Store::UrlFileStore::LoadIndexQueue()
{
	std::unique_ptr<Iterator> iter(mDb->NewIterator(ReadOptions(), mCFHandles[IndexQueueCF].get()));

	for (iter->SeekToFirst(); iter->Valid(); iter->Next())
	{
		auto id = SerializeHelper::DeserializeUInt64(iter->value().ToString());
		UrlFile urlFile;
		if (GetUrlFile(id, urlFile))
		{
			AddToIndexQueue(urlFile);
		}
	}
}

void XiaoyaStore::Store::UrlFileStore::AddToIndexQueue(Model::UrlFile & urlFile)
{
	std::unique_lock<std::shared_mutex> lock(mIndexQueueMutex);

	mIndexQueue.push(urlFile);
}

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
{
	LoadIndexQueue();
}

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
		result.reserve(idList.Ids.size());

		for (auto id : idList.Ids)
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
	if (GetUrlFile(urlFile.Url, oldUrlFile))
	{
		// Exists old UrlFile with the same Url
		oldUrlFileId = oldUrlFile.UrlFileId;

		auto updateInterval = now - oldUrlFile.UpdatedAt;

		urlFile.CreatedAt = oldUrlFile.CreatedAt;
		urlFile.UpdateInterval =
			static_cast<uint64_t>((oldUrlFile.UpdateInterval * 3.0 + updateInterval) / 4.0);

		if (oldUrlFile.Title != urlFile.Title
			|| oldUrlFile.Content != urlFile.Content)
		{
			// Title or Content changed
			urlFile.UpdatedAt = now;
			willAddToIndexQueue = true;
		}
		else
		{
			urlFile.UpdatedAt = oldUrlFile.UpdatedAt;
		}

		if (oldUrlFile.FileHash != urlFile.FileHash)
		{
			// Hash changed, remove old hash index
			IdList removingHashIndex;
			removingHashIndex.IsAdd = false;
			removingHashIndex.Ids = std::set<uint64_t>();
			removingHashIndex.Ids.insert(oldUrlFile.UrlFileId);

			batch.Merge(mCFHandles[HashIndexCF].get(), oldUrlFile.FileHash,
				SerializeHelper::Serialize(removingHashIndex));
		}

		// Delete old UrlFile
		batch.Delete(SerializeHelper::SerializeUInt64(oldUrlFile.UrlFileId));
	}
	else
	{
		// New Url
		oldUrlFileId = -1;

		urlFile.UpdatedAt = now;
		urlFile.CreatedAt = now;
		urlFile.UpdateInterval = DateTimeHelper::FromDays(1);

		willAddToIndexQueue = true;
	}

	// Assign new id
	auto id = urlFile.UrlFileId = GetAndUpdateValue(MetaMaxUrlFileId, 1) + 1;
	auto idData = SerializeHelper::SerializeUInt64(id);

	// Overwrite url index
	batch.Put(mCFHandles[UrlIndexCF].get(), urlFile.Url, idData);

	// Add new hash index
	IdList newHashIndex;
	newHashIndex.IsAdd = true;
	newHashIndex.Ids = std::set<uint64_t>();
	newHashIndex.Ids.insert(id);

	batch.Merge(mCFHandles[HashIndexCF].get(), urlFile.FileHash,
		SerializeHelper::Serialize(newHashIndex));

	// Add new UrlFile
	batch.Put(idData, SerializeHelper::Serialize(urlFile));

	if (willAddToIndexQueue)
	{
		// Add to IndexQueue
		batch.Put(mCFHandles[IndexQueueCF].get(), urlFile.Url, idData);
		AddToIndexQueue(urlFile);
	}

	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status,
			"UrlFileStore::SaveUrlFileAndGetOldId failed to save: " + urlFile.Url);
	}

	return oldUrlFileId;
}

uint64_t UrlFileStore::GetCount() const
{
	return GetMaxUrlFileId();
}

bool UrlFileStore::GetForIndex(UrlFile &outUrlFile) const
{
	std::shared_lock<std::shared_mutex> lock(mIndexQueueMutex);
	if (mIndexQueue.empty())
	{
		return false;
	}
	
	outUrlFile = mIndexQueue.top();
	mIndexQueue.pop();

	return true;
}

void XiaoyaStore::Store::UrlFileStore::FinishIndex(std::string &url)
{
	auto status = mDb->Delete(WriteOptions(), mCFHandles[IndexQueueCF].get(), url);
	if (!status.ok() && !status.IsNotFound())
	{
		throw StoreException(status,
			"UrlFileStore::FinishIndex failed to remove url: " + url);
	}
}

const std::vector<rocksdb::ColumnFamilyDescriptor>
UrlFileStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions idListOptions;
	idListOptions.merge_operator.reset(new MergeOperator::IdListOperator());

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, ColumnFamilyOptions()),
			ColumnFamilyDescriptor(IndexQueueCFName, idListOptions),
			ColumnFamilyDescriptor(UrlIndexCFName, idListOptions),
			ColumnFamilyDescriptor(HashIndexCFName, idListOptions),
	});
}
