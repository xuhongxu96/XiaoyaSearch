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

const std::string CounterBaseStore::CounterCFName = "id";

const std::vector<ColumnFamilyDescriptor> 
CounterBaseStore::AddIdColumnFamilyDescriptor(
	const std::vector<ColumnFamilyDescriptor>& columnFamilyDescriptors
)
{
	ColumnFamilyOptions counterOptions;
	counterOptions.merge_operator.reset(new MergeOperator::CounterOperator());

	auto result = columnFamilyDescriptors;
	result.push_back(ColumnFamilyDescriptor(CounterCFName, counterOptions));
	return result;
}

uint64_t CounterBaseStore::GetValueInternal(const std::string &key) const
{
	std::string data;
	auto status = mDb->Get(ReadOptions(), mCFHandles[GetCounterCF()].get(), key, &data);
	if (status.IsNotFound())
	{
		return 0;
	}
	else if (!status.ok())
	{
		throw StoreException(status, "CounterBaseStore::GetValue failed to get value of " + key);
	}
	return SerializeHelper::DeserializeUInt64(data);
}

void CounterBaseStore::UpdateValueInternal(const std::string &key, int64_t delta)
{
	auto data = SerializeHelper::SerializeInt64(delta);
	auto status = mDb->Merge(WriteOptions(), mCFHandles[GetCounterCF()].get(), key, data);
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

	UpdateValueInternal(key, delta);
}

uint64_t CounterBaseStore::GetAndUpdateValue(const std::string &key, int64_t delta)
{
	std::unique_lock<std::shared_mutex> lock(mCounterMutex);

	auto result = GetValueInternal(key);
	UpdateValueInternal(key, delta);

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
{ }
