#include "stdafx.h"
#include "LinkStore.h"
#include "UrlHelper.h"
#include "DateTimeHelper.h"
#include "StoreException.h"
#include "GenericSetOperator.h"

using namespace rocksdb;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Exception;

const std::string LinkStore::DbName = "LinkStore";

const std::string LinkStore::UrlFileIdIndexCFName = "url_file_id_index";
const size_t LinkStore::UrlFileIdIndexCF = 1;

LinkStore::LinkStore(StoreConfig config, bool isReadOnly)
	: BaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

void LinkStore::SaveLinks(const uint64_t urlFileId,
	const std::vector<Link> &links)
{
	WriteBatch batch;

	Links linksOfUrlFile;

	// Add links
	for (auto link : links)
	{
		Links deltaLinks;
		deltaLinks.set_is_add(true);
		*deltaLinks.add_items() = link;
		*linksOfUrlFile.add_items() = link;

		batch.Merge(link.url(), SerializeHelper::Serialize(deltaLinks));
	}

	// Save UrlFileId Index
	batch.Put(mCFHandles[UrlFileIdIndexCF].get(),
		SerializeHelper::SerializeUInt64(urlFileId),
		SerializeHelper::Serialize(linksOfUrlFile));

	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "LinkStore::SaveLinks failed to save.");
	}
}

void LinkStore::ClearLinks(const uint64_t urlFileId)
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
		throw StoreException(status, "LinkStore::ClearLinks failed to get links for "
			+ std::to_string(urlFileId));
	}

	auto links = SerializeHelper::Deserialize<Links>(data);

	WriteBatch batch;

	for (auto link : links.items())
	{
		Links deltaLinks;
		deltaLinks.set_is_add(false);
		*deltaLinks.add_items() = link;

		batch.Merge(link.url(), SerializeHelper::Serialize(deltaLinks));
	}
	batch.Delete(mCFHandles[UrlFileIdIndexCF].get(), key);

	status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "LinkStore::ClearLinks failed to clear links for "
			+ std::to_string(urlFileId));
	}
}

std::vector<Link> LinkStore::GetLinks(const std::string &url) const
{
	std::string data;

	auto status = mDb->Get(ReadOptions(), url, &data);
	if (status.IsNotFound())
	{
		return std::vector<Link>();
	}
	else if (!status.ok())
	{
		throw StoreException(status, "LinkStore::GetLinksByUrl failed to get links for: " + url);
	}

	auto links = SerializeHelper::Deserialize<Links>(data);
	return std::vector<Link>(links.items().begin(), links.items().end());
}

const std::vector<rocksdb::ColumnFamilyDescriptor>
LinkStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions options;
	options.max_successive_merges = 1000;
	options.merge_operator.reset(new MergeOperator::GenericSetOperator<Links, Link, ModelCompare>());

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, options),
			ColumnFamilyDescriptor(UrlFileIdIndexCFName, ColumnFamilyOptions()),
	});
}
