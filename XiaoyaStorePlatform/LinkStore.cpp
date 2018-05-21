#include "stdafx.h"
#include "LinkStore.h"
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

const std::string LinkStore::DbName = "LinkStore";

const std::string LinkStore::MetaMaxLinkId = "MaxLinkId";

const std::string LinkStore::UrlIndexCFName = "url_index";
const size_t LinkStore::UrlIndexCF = 1;

const std::string LinkStore::UrlFileIdIndexCFName = "urlfile_id_index";
const size_t LinkStore::UrlFileIdIndexCF = 2;


bool LinkStore::GetLinkIds(const std::string &url, IdList &outIdList) const
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[UrlIndexCF].get(), url, &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status,
			"LinkStore::GetLinkIds failed to get link id list of url: " + url);
	}

	outIdList = SerializeHelper::Deserialize<IdList>(data);
	return true;
}

bool LinkStore::GetLinkIds(const uint64_t urlFileId, IdList & outIdList) const
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
		throw StoreException(status,
			"LinkStore::GetLinkIds failed to get link id list of urlFileId: "
			+ std::to_string(urlFileId));
	}

	outIdList = SerializeHelper::Deserialize<IdList>(data);
	return true;
}

void LinkStore::ClearLinksOfUrlFile(const uint64_t urlFileId, WriteBatch &batch)
{
	IdList oldLinkIdList;
	if (GetLinkIds(urlFileId, oldLinkIdList))
	{
		for (auto oldLinkId : oldLinkIdList.Ids)
		{
			// Remove old url index
			Link oldLink;
			if (GetLink(oldLinkId, oldLink))
			{
				IdList deltaUrlIndex;
				deltaUrlIndex.IsAdd = false;
				deltaUrlIndex.Ids = { oldLink.LinkId };
				batch.Merge(mCFHandles[UrlIndexCF].get(), oldLink.Url,
					SerializeHelper::Serialize(deltaUrlIndex));
			}

			// Remove old link
			batch.Delete(SerializeHelper::SerializeUInt64(oldLinkId));
		}
	}

	// Remove UrlFileId index
	batch.Delete(mCFHandles[UrlFileIdIndexCF].get(),
		SerializeHelper::SerializeUInt64(urlFileId));
}

LinkStore::LinkStore(StoreConfig config, bool isReadOnly)
	: CounterBaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

void LinkStore::SaveLinksOfUrlFile(const uint64_t urlFileId,
	const uint64_t oldUrlFileId, std::vector<Link> links)
{
	WriteBatch batch;

	if (oldUrlFileId > 0)
	{
		ClearLinksOfUrlFile(oldUrlFileId, batch);
	}

	IdList deltaUrlFileIdIndex;
	deltaUrlFileIdIndex.IsAdd = true;
	deltaUrlFileIdIndex.Ids = std::set<uint64_t>();

	// Add links and url index
	for (auto link : links)
	{
		// Assign new link id
		link.LinkId = GetAndUpdateValue(MetaMaxLinkId, 1) + 1;

		// Add new url index
		IdList deltaUrlIndex;
		deltaUrlIndex.IsAdd = true;
		deltaUrlIndex.Ids = { link.LinkId };
		batch.Merge(mCFHandles[UrlIndexCF].get(), link.Url,
			SerializeHelper::Serialize(deltaUrlIndex));

		// Add link
		batch.Put(SerializeHelper::SerializeUInt64(link.LinkId),
			SerializeHelper::Serialize(link));

		// Add UrlFileId index
		deltaUrlFileIdIndex.Ids.insert(link.LinkId);
	}

	// Update UrlFileId index
	batch.Put(mCFHandles[UrlFileIdIndexCF].get(),
		SerializeHelper::SerializeUInt64(urlFileId),
		SerializeHelper::Serialize(deltaUrlFileIdIndex));

	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "LinkStore::SaveLinksOfUrlFile failed to save for urlFileId: "
			+ std::to_string(urlFileId));
	}
}

bool LinkStore::GetLink(const uint64_t id, Link & outLink) const
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), SerializeHelper::SerializeUInt64(id), &data);
	if (status.IsNotFound())
	{
		return false;
	}
	else if (!status.ok())
	{
		throw StoreException(status,
			"LinkStore::GetLink failed to get id: " + std::to_string(id));
	}
	outLink = SerializeHelper::Deserialize<Link>(data);
	return true;
}

std::vector<Link> LinkStore::GetLinksByUrl(const std::string &url) const
{
	std::vector<Link> result;

	IdList idList;
	if (GetLinkIds(url, idList))
	{
		for (auto id : idList.Ids)
		{
			Link link;
			if (GetLink(id, link))
			{
				result.push_back(link);
			}
		}
	}
	return result;
}

const std::vector<rocksdb::ColumnFamilyDescriptor>
LinkStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions idListOptions;
	idListOptions.merge_operator.reset(new MergeOperator::IdListOperator());
	idListOptions.max_successive_merges = 1000;

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, ColumnFamilyOptions()),
			ColumnFamilyDescriptor(UrlIndexCFName, idListOptions),
			ColumnFamilyDescriptor(UrlFileIdIndexCFName, idListOptions),
	});
}
