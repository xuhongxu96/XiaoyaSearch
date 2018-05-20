#pragma once

#include "pch.h"

class DbTestHelper
{
public:
	static const std::string storeDir;

	template <typename T>
	static void OpenDB(std::unique_ptr<rocksdb::DB> &db,
		std::vector<std::unique_ptr<rocksdb::ColumnFamilyHandle>> &handles)
	{
		// Unmanaged pointers to get values from API call
		rocksdb::DB* db_ptr;
		std::vector<rocksdb::ColumnFamilyHandle*> handle_ptrs;

		auto cfd = std::is_base_of<CounterBaseStore, T>::value ?
			XiaoyaStore::Store::CounterBaseStore::AddIdColumnFamilyDescriptor(T::GetColumnFamilyDescriptors())
			: T::GetColumnFamilyDescriptors();

		auto status = DB::Open(Options(), storeDir + "\\" + T::DbName, cfd, &handle_ptrs, &db_ptr);

		if (!status.ok())
		{
			throw XiaoyaStore::Exception::StoreException(status, "Failed to open DB: " + T::DbName);
		}

		// Manage pointers using std::unique_ptr
		db.reset(db_ptr);

		handles.resize(handle_ptrs.size());
		for (int i = 0; i < handles.size(); ++i)
		{
			handles[i] = std::move(std::unique_ptr<rocksdb::ColumnFamilyHandle>(handle_ptrs[i]));
		}
	}

	template <typename T>
	static void DeleteDB()
	{
		namespace fs = boost::filesystem;

		fs::path dbPath(storeDir + "\\" + T::DbName);
		if (fs::exists(dbPath))
		{
			fs::remove_all(dbPath);
		}
	}

	static XiaoyaStore::Config::StoreConfig InitStoreConfig()
	{
		XiaoyaStore::Config::StoreConfig config;
		config.StoreDirectory = storeDir;
		return config;
	}
};