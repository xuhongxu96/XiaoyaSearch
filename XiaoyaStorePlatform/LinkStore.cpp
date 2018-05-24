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
		for (auto oldLinkId : oldLinkIdList.ids())
		{
			// Remove old url index
			Link oldLink;
			if (GetLink(oldLinkId, oldLink))
			{
				IdList deltaUrlIndex;
				deltaUrlIndex.set_is_add(false);
				deltaUrlIndex.add_ids(oldLink.link_id());

				batch.Merge(mCFHandles[UrlIndexCF].get(), oldLink.url(),
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
	const uint64_t oldUrlFileId, const std::vector<Link> &links)
{
	WriteBatch batch;

	if (oldUrlFileId > 0)
	{
		ClearLinksOfUrlFile(oldUrlFileId, batch);
	}

	IdList deltaUrlFileIdIndex;
	deltaUrlFileIdIndex.set_is_add(true);

	// Add links and url index
	for (auto link : links)
	{
		// Assign new link id
		auto id = GetAndUpdateValue(MetaMaxLinkId, 1) + 1;
		link.set_link_id(id);

		// Add new url index
		IdList deltaUrlIndex;
		deltaUrlIndex.set_is_add(true);
		deltaUrlIndex.add_ids(id);

		batch.Merge(mCFHandles[UrlIndexCF].get(), link.url(),
			SerializeHelper::Serialize(deltaUrlIndex));

		// Add link
		batch.Put(SerializeHelper::SerializeUInt64(id),
			SerializeHelper::Serialize(link));

		// Add UrlFileId index
		deltaUrlFileIdIndex.add_ids(id);
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
		for (auto id : idList.ids())
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
