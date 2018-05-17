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

const std::vector<ColumnFamilyDescriptor> UrlFrontierItemStore::GetColumnFamilyDescriptors()
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
	std::cout << "Loading UrlFrontierItems" << std::endl;

	std::unique_ptr<Iterator> iter(mDb->NewIterator(ReadOptions()));
	for (iter->SeekToFirst(); iter->Valid(); iter->Next())
	{
		auto slice = iter->value();
		auto item = SerializerHelper::Deserialize<UrlFrontierItem>(slice.ToString());

		AddToQueue(item);
	}

	std::cout << "Loaded UrlFrontierItems" << std::endl;
}

UrlFrontierItem UrlFrontierItemStore::CreateItem(const std::string & url) const
{
	auto now = DateTimeHelper::Now();

	UrlFrontierItem item;
	item.Url = url;
	item.PlannedTime = now;
	item.FailedTimes = 0;
	item.UpdatedAt = now;
	item.CreatedAt = now;

	return std::move(item);
}

void UrlFrontierItemStore::SaveNewItem(UrlFrontierItem &item)
{
	WriteBatch batch;

	auto host = UrlHelper::GetHost(item.Url);

	auto data = SerializerHelper::Serialize(item);

	batch.Put(item.Url, data);
	batch.Merge(mCFHandles[HostCountCF].get(), host, SerializerHelper::SerializeInt64(1));

	auto status = mDb->Write(WriteOptions(), &batch);

	if (!status.ok())
	{
		throw StoreException("Failed to save new item (" + item.Url + "): " + status.ToString());
	}
}

void XiaoyaStore::Store::UrlFrontierItemStore::UpdateItem(Model::UrlFrontierItem & item)
{
	auto data = SerializerHelper::Serialize(item);
	auto status = mDb->Put(WriteOptions(), item.Url, data);
	if (!status.ok())
	{
		throw StoreException("Failed to update item (" + item.Url + "): " + status.ToString());
	}
}

void XiaoyaStore::Store::UrlFrontierItemStore::AddToQueue(UrlFrontierItem & item)
{
	std::unique_lock<std::shared_mutex> lock(mSharedMutexForQueue);

	mUrlQueue.push(item);
	mUrlSet.insert(item.Url);
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

		item.Priority = hostCount + depth * 10;

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
		item.FailedTimes++;
		item.PlannedTime = DateTimeHelper::Now()
			+ DateTimeHelper::FromDays(item.FailedTimes);
	}
	else
	{
		// Successfully fetched
		item.FailedTimes = 0;
		item.PlannedTime = DateTimeHelper::Now() + updateInterval;
	}
	item.UpdatedAt = DateTimeHelper::Now();

	auto depth = UrlHelper::GetDomainDepth(url);
	auto host = UrlHelper::GetHost(url);
	auto hostCount = GetHostCount(host);

	item.Priority = hostCount + depth * 10;

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

	if (mPoppedUrlMap.count(item.Url) > 0)
	{
		return false;
	}

	url = item.Url;
	mPoppedUrlMap.insert(std::make_pair(url, std::move(item)));

	return true;
}

void XiaoyaStore::Store::UrlFrontierItemStore::RemoveUrl(std::string & url)
{
	WriteBatch batch;

	auto host = UrlHelper::GetHost(url);

	batch.Delete(url);
	batch.Merge(mCFHandles[HostCountCF].get(), host, SerializerHelper::SerializeInt64(-1));
	auto status = mDb->Write(WriteOptions(), &batch);
	if (!status.ok())
	{
		throw StoreException("Failed to remove url (" + url + "): " + status.ToString());
	}
}

uint64_t XiaoyaStore::Store::UrlFrontierItemStore::GetHostCount(const std::string host)
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[HostCountCF].get(), host, &data);
	if (status.IsNotFound())
	{
		return 0;
	}
	else if (!status.ok())
	{
		throw StoreException("Failed to get host count (" + host + "): " + status.ToString());
	}
	return SerializerHelper::DeserializeUInt64(data);
}
