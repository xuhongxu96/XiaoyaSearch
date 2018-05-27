#include "stdafx.h"
#include "UrlFrontierItemStore.h"
#include "UrlHelper.h"
#include "DateTimeHelper.h"
#include "StoreException.h"
#include "CounterOperator.h"

using namespace rocksdb;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Exception;

const std::string UrlFrontierItemStore::DbName = "UrlFrontierItemStore";

const std::string UrlFrontierItemStore::HostCountCFName = "host_count";
const size_t UrlFrontierItemStore::HostCountCF = 1;

const std::vector<ColumnFamilyDescriptor>
UrlFrontierItemStore::GetColumnFamilyDescriptors()
{
	ColumnFamilyOptions hostCountOptions;
	hostCountOptions.merge_operator.reset(new MergeOperator::CounterOperator());

	return std::move(std::vector<ColumnFamilyDescriptor>
	{
		ColumnFamilyDescriptor(BaseStore::DefaultCFName, ColumnFamilyOptions()),
			ColumnFamilyDescriptor(HostCountCFName, hostCountOptions),
	});
}

void UrlFrontierItemStore::LoadUrlFrontierItems()
{
	std::unique_ptr<Iterator> iter(mDb->NewIterator(ReadOptions()));
	for (iter->SeekToFirst(); iter->Valid(); iter->Next())
	{
		auto item = SerializeHelper::Deserialize<UrlFrontierItem>(iter->value().ToString());

		AddToQueue(item);
	}
}

UrlFrontierItem UrlFrontierItemStore::CreateItem(const std::string & url) const
{
	auto now = DateTimeHelper::Now();

	UrlFrontierItem item;
	item.set_url(url);
	item.set_planned_time(now);
	item.set_failed_times(0);
	item.set_updated_at(now);
	item.set_created_at(now);

	return std::move(item);
}

void UrlFrontierItemStore::SaveNewItem(UrlFrontierItem &item)
{
	WriteBatch batch;

	auto host = UrlHelper::GetHost(item.url());

	auto data = SerializeHelper::Serialize(item);

	batch.Put(item.url(), data);
	batch.Merge(mCFHandles[HostCountCF].get(), host, SerializeHelper::SerializeInt64(1));

	auto status = mDb->Write(WriteOptions(), &batch);

	if (!status.ok())
	{
		throw StoreException(status, 
			"UrlFrontierItemStore::SaveNewItem failed to save new item (" 
			+ item.url() + "): " + status.ToString());
	}
}

void XiaoyaStore::Store::UrlFrontierItemStore::UpdateItem(Model::UrlFrontierItem & item)
{
	auto data = SerializeHelper::Serialize(item);
	auto status = mDb->Put(WriteOptions(), item.url(), data);
	if (!status.ok())
	{
		throw StoreException(status, 
			"UrlFrontierItemStore::UpdateItem failed to update item ("
			+ item.url() + "): " + status.ToString());
	}
}

void XiaoyaStore::Store::UrlFrontierItemStore::AddToQueue(UrlFrontierItem & item)
{
	std::unique_lock<std::shared_mutex> lock(mSharedMutexForQueue);

	mUrlQueue.push(item);
	mUrlSet.insert(item.url());
}

bool XiaoyaStore::Store::UrlFrontierItemStore::HasUrl(const std::string & url) const
{
	std::shared_lock<std::shared_mutex> lock(mSharedMutexForQueue);

	return mUrlSet.count(url) > 0;
}

bool XiaoyaStore::Store::UrlFrontierItemStore::HasUrlOrIsPopped(const std::string & url) const
{
	std::shared_lock<std::shared_mutex> lock(mSharedMutexForPop);

	return mUrlSet.count(url) > 0 || mPoppedUrlMap.count(url) > 0;
}

bool XiaoyaStore::Store::UrlFrontierItemStore::IsPopped(const std::string & url) const
{
	std::shared_lock<std::shared_mutex> lock(mSharedMutexForPop);

	return mPoppedUrlMap.count(url) > 0;
}

bool XiaoyaStore::Store::UrlFrontierItemStore::TryRemovePoppedUrl(const std::string & url, Model::UrlFrontierItem & outItem)
{
	std::unique_lock<std::shared_mutex> lock(mSharedMutexForPop);

	if (mPoppedUrlMap.count(url) > 0)
	{
		outItem = std::move(mPoppedUrlMap[url]);
		if (mPoppedUrlMap.erase(url) != 0)
		{
			return true;
		}
	}
	return false;
}

UrlFrontierItemStore::UrlFrontierItemStore(StoreConfig config, bool isReadOnly)
	: BaseStore(DbName, GetColumnFamilyDescriptors(), config, isReadOnly)
{
	LoadUrlFrontierItems();
}

void UrlFrontierItemStore::Init(const std::vector<std::string>& urls)
{
	for (auto url : urls)
	{
		if (HasUrl(url))
		{
			// Already exists, skip
			continue;
		}

		auto item = CreateItem(url);

		SaveNewItem(item);
		AddToQueue(item);
	}
}

void UrlFrontierItemStore::PushUrls(const std::vector<std::string>& urls)
{
	for (auto url : urls)
	{
		if (HasUrlOrIsPopped(url))
		{
			// Already exists or is popped, skip
			continue;
		}

		auto item = CreateItem(url);

		auto host = UrlHelper::GetHost(url);
		auto depth = UrlHelper::GetDomainDepth(url);

		auto hostCount = GetHostCount(host);

		item.set_priority(hostCount + depth * 10);

		SaveNewItem(item);
		AddToQueue(item);
	}
}

bool UrlFrontierItemStore::PushBackUrl(const std::string url,
	uint64_t updateInterval, bool failed)
{
	if (!IsPopped(url))
	{
		// Url is not popped, no need to push back, skip
		return false;
	}

	UrlFrontierItem item;

	if (!TryRemovePoppedUrl(url, item))
	{
		// Failed to remove
		return false;
	}

	// Set next PlannedTime and other props
	if (failed)
	{
		// Failed to fetch
		item.set_failed_times(item.failed_times() + 1);
		item.set_planned_time(DateTimeHelper::Now()
			+ DateTimeHelper::FromDays(item.failed_times()));
	}
	else
	{
		// Successfully fetched
		item.set_failed_times(0);
		item.set_planned_time(DateTimeHelper::Now() + updateInterval);
	}
	item.set_updated_at(DateTimeHelper::Now());

	auto depth = UrlHelper::GetDomainDepth(url);
	auto host = UrlHelper::GetHost(url);
	auto hostCount = GetHostCount(host);

	item.set_priority(hostCount + depth * 10);

	UpdateItem(item);
	AddToQueue(item);

	return true;
}

const bool XiaoyaStore::Store::UrlFrontierItemStore::PopUrl(std::string &url)
{
	std::unique_lock<std::shared_mutex> lockForQueue(mSharedMutexForQueue);
	std::unique_lock<std::shared_mutex> lockForPop(mSharedMutexForPop);

	if (mUrlQueue.empty())
	{
		return false;
	}

	auto item = mUrlQueue.top();
	mUrlQueue.pop();

	if (mPoppedUrlMap.count(item.url()) > 0)
	{
		return false;
	}

	url = item.url();
	mPoppedUrlMap.insert(std::make_pair(url, std::move(item)));

	return true;
}

void XiaoyaStore::Store::UrlFrontierItemStore::RemoveUrl(const std::string & url)
{
	WriteBatch batch;

	auto host = UrlHelper::GetHost(url);

	batch.Delete(url);
	batch.Merge(mCFHandles[HostCountCF].get(), host, SerializeHelper::SerializeInt64(-1));
	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException(status, "UrlFrontierItemStore::RemoveUrl failed to remove url (" + url + "): " + status.ToString());
	}
}

uint64_t XiaoyaStore::Store::UrlFrontierItemStore::GetHostCount(const std::string &host)
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[HostCountCF].get(), host, &data);
	if (status.IsNotFound())
	{
		return 0;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "UrlFrontierItemStore::GetHostCount failed to get host count (" + host + "): " + status.ToString());
	}
	return SerializeHelper::DeserializeUInt64(data);
}
