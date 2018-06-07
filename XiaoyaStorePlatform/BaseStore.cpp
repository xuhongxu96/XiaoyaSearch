#include "stdafx.h"
#include "BaseStore.h"
#include "StoreException.h"

namespace fs = boost::filesystem;

using namespace rocksdb;
using namespace XiaoyaStore::Config;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Exception;

const std::string BaseStore::DefaultCFName = "default";
const size_t BaseStore::DefaultCF = 0;

void BaseStore::OpenDb(const std::vector<rocksdb::ColumnFamilyDescriptor> &columnFamilyDescriptors)
{
	// Create options
	Options options;
	options.create_if_missing = true;
	options.create_missing_column_families = true;

	// Unmanaged pointers to get values from API call
	DB* db;
	std::vector<ColumnFamilyHandle*> handles;
	
	Status status;

	// Open RocksDB
	if (mIsReadOnly)
	{
		status = DB::OpenForReadOnly(options, mDbPath, columnFamilyDescriptors, &handles, &db);
	}
	else
	{
		status = DB::Open(options, mDbPath, columnFamilyDescriptors, &handles, &db);
	}

	if (db == nullptr)
	{
		throw StoreException(status, "Failed to open db: " + mDbPath);
	}

	// Manage pointers using std::unique_ptr
	mDb.reset(db);
	
	mCFHandles.resize(handles.size());
	for (int i = 0; i < handles.size(); ++i)
	{
		mCFHandles[i] = std::move(std::unique_ptr<ColumnFamilyHandle>(handles[i]));
	}
}

void XiaoyaStore::Store::BaseStore::SetDbPath(const std::string &dbDirectory,
	const std::string &dbFileName)
{
	// Create boost::filesystem::path
	fs::path storeDir(dbDirectory);
	fs::path dbFile(dbFileName);

	// Create StoreDirectory if not exists
	if (!fs::exists(storeDir))
	{
		fs::create_directory(dbDirectory);
	}

	// Combine path to get DbPath
	mDbPath = (storeDir / dbFile).string();
}

BaseStore::BaseStore(
	const std::string &dbFileName,
	const std::vector<rocksdb::ColumnFamilyDescriptor> &columnFamilyDescriptors,
	StoreConfig config,
	bool isReadOnly
) : mDbFileName(dbFileName), mConfig(config), mIsReadOnly(isReadOnly)
{
	SetDbPath(config.StoreDirectory, dbFileName);
	OpenDb(columnFamilyDescriptors);
}
