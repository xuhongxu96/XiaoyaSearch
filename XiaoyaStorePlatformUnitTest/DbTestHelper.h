#pragma once

#include "pch.h"

namespace DbTestHelper
{
	const std::string storeDir = boost::filesystem::current_path().string();

	template <typename T>
	void OpenDB(std::unique_ptr<rocksdb::DB> &db,
		std::vector<std::unique_ptr<rocksdb::ColumnFamilyHandle>> &handles)
	{
		// Unmanaged pointers to get values from API call
		rocksdb::DB* db_ptr;
		std::vector<rocksdb::ColumnFamilyHandle*> handle_ptrs;

		auto status = DB::Open(Options(), storeDir + "\\" + T::DbName, T::GetColumnFamilyDescriptors(), &handle_ptrs, &db_ptr);

		// Manage pointers using std::unique_ptr
		db.reset(db_ptr);

		handles.resize(handle_ptrs.size());
		for (int i = 0; i < handles.size(); ++i)
		{
			handles[i] = std::move(std::unique_ptr<rocksdb::ColumnFamilyHandle>(handle_ptrs[i]));
		}
	}

	template <typename T>
	void DeleteDB()
	{
		namespace fs = boost::filesystem;

		fs::path dbPath(storeDir + "\\" + T::DbName);
		if (fs::exists(dbPath))
		{
			fs::remove_all(dbPath);
		}
	}

	XiaoyaStore::Config::StoreConfig InitStoreConfig()
	{
		XiaoyaStore::Config::StoreConfig config;
		config.StoreDirectory = storeDir;
		return config;
	}
}