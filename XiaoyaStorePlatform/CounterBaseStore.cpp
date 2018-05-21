#include "stdafx.h"
#include "CounterBaseStore.h"
#include "SerializeHelper.h"
#include "CounterOperator.h"
#include "CounterBaseStore.h"
#include "StoreException.h"

using namespace rocksdb;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Exception;

const std::string CounterBaseStore::CounterCFName = "counter";

const std::vector<ColumnFamilyDescriptor> 
CounterBaseStore::AddIdColumnFamilyDescriptor(
	const std::vector<ColumnFamilyDescriptor>& columnFamilyDescriptors
)
{
	ColumnFamilyOptions counterOptions;
	// counterOptions.merge_operator.reset(new MergeOperator::CounterOperator());

	auto result = columnFamilyDescriptors;
	result.push_back(ColumnFamilyDescriptor(CounterCFName, counterOptions));
	return result;
}

void XiaoyaStore::Store::CounterBaseStore::LoadCounter()
{
	std::unique_lock<std::shared_mutex> lock(mCounterMutex);

	std::unique_ptr<Iterator> iter(mDb->NewIterator(ReadOptions(), mCFHandles[GetCounterCF()].get()));
	for (iter->SeekToFirst(); iter->Valid(); iter->Next())
	{
		auto value = SerializeHelper::DeserializeUInt64(iter->value().ToString());

		mCounterMap[iter->key().ToString()] = value;
	}
}

uint64_t CounterBaseStore::GetValueInternal(const std::string &key) const
{
	if (mCounterMap.count(key) == 0)
	{
		return 0;
	}
	return mCounterMap.at(key);
}

void CounterBaseStore::UpdateValueInMemoryInternal(const std::string &key, int64_t delta)
{
	if (mCounterMap.count(key) == 0)
	{
		mCounterMap[key] = delta;
	}
	else
	{
		mCounterMap[key] += delta;
	}
}

void CounterBaseStore::UpdateValueInDatabaseInternal(const std::string &key, uint64_t value)
{
	auto data = SerializeHelper::SerializeInt64(value);
	auto status = mDb->Put(WriteOptions(), mCFHandles[GetCounterCF()].get(), key, data);
	if (!status.ok())
	{
		throw StoreException(status, "CounterBaseStore::UpdateValue failed to update value of " + key);
	}
}

uint64_t CounterBaseStore::GetValue(const std::string &key) const
{
	std::shared_lock<std::shared_mutex> lock(mCounterMutex);

	return GetValueInternal(key);
}

void CounterBaseStore::UpdateValue(const std::string &key, int64_t delta)
{
	std::unique_lock<std::shared_mutex> lock(mCounterMutex);

	UpdateValueInMemoryInternal(key, delta);
	UpdateValueInDatabaseInternal(key, mCounterMap[key]);
}

uint64_t CounterBaseStore::GetAndUpdateValue(const std::string &key, int64_t delta)
{
	std::unique_lock<std::shared_mutex> lock(mCounterMutex);

	auto result = GetValueInternal(key);

	UpdateValueInMemoryInternal(key, delta);
	UpdateValueInDatabaseInternal(key, mCounterMap[key]);

	return result;
}

const size_t CounterBaseStore::GetCounterCF() const
{
	return mCounterCF;
}

CounterBaseStore::CounterBaseStore(const std::string & dbFileName,
	const std::vector<ColumnFamilyDescriptor>& columnFamilyDescriptors,
	StoreConfig config, 
	bool isReadOnly)
	: BaseStore(dbFileName, AddIdColumnFamilyDescriptor(columnFamilyDescriptors), 
		config, isReadOnly), mCounterCF(columnFamilyDescriptors.size())
{
	LoadCounter();
}
