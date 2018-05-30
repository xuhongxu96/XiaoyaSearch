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

LinkStore::LinkStore(StoreConfig config, bool isReadOnly)
	: BaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{ }

void LinkStore::SaveLinks(const std::vector<Link> &links)
{
	WriteBatch batch;

	// Add links
	for (auto link : links)
	{
		Links deltaLinks;
		deltaLinks.set_is_add(true);
		*deltaLinks.add_items() = link;

		batch.Merge(link.url(), SerializeHelper::Serialize(deltaLinks));
	}

	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "LinkStore::SaveLinks failed to save.");
	}
}

void LinkStore::RemoveLinks(const std::vector<Link> &links)
{
	WriteBatch batch;

	// Add links
	for (auto link : links)
	{
		Links deltaLinks;
		deltaLinks.set_is_add(false);
		*deltaLinks.add_items() = link;

		batch.Merge(link.url(), SerializeHelper::Serialize(deltaLinks));
	}

	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "LinkStore::RemoveLinks failed to remove.");
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
	});
}
