#include "pch.h"

#include "../XiaoyaStorePlatform/StoreConfig.h"
#include "../XiaoyaStorePlatform/UrlFrontierItemStore.h"
#include "../XiaoyaStorePlatform/SerializerHelper.h"

using namespace XiaoyaStore;
using namespace rocksdb;

Config::StoreConfig InitConfig()
{
	Config::StoreConfig config;
	config.StoreDirectory = "D:\\Store";
	return config;
}

template <typename T>
void OpenDB(std::unique_ptr<DB> &db, std::vector<std::unique_ptr<ColumnFamilyHandle>> &handles)
{
	// Unmanaged pointers to get values from API call
	DB* db_ptr;
	std::vector<ColumnFamilyHandle*> handle_ptrs;

	auto status = DB::Open(Options(), "D:\\Store\\" + T::DbName, T::GetColumnFamilyDescriptors(), &handle_ptrs, &db_ptr);

	// Manage pointers using std::unique_ptr
	db.reset(db_ptr);

	handles.resize(handle_ptrs.size());
	for (int i = 0; i < handles.size(); ++i)
	{
		handles[i] = std::move(std::unique_ptr<ColumnFamilyHandle>(handle_ptrs[i]));
	}
}

TEST(TestInit, TestUrlFrontierItemStore) {

	auto config = InitConfig();

	// Init UrlFrontierStore with urls
	{
		Store::UrlFrontierItemStore store(config);
		store.Init({
			"http://baidu.com",
			"http://baidu.com/a",
			"http://xuhongxu.com",
		});
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		OpenDB<Store::UrlFrontierItemStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = Helper::SerializerHelper::Deserialize<Model::UrlFrontierItem>(iter->value().ToString());
			items.insert(item.Url);
		}

		ASSERT_EQ(1, items.count("http://baidu.com"));
		ASSERT_EQ(1, items.count("http://baidu.com/a"));
		ASSERT_EQ(1, items.count("http://xuhongxu.com"));
		ASSERT_EQ(0, items.count("http://google.com"));

		std::map<std::string, uint64_t> host;

		std::unique_ptr<Iterator> host_iter(db->NewIterator(ReadOptions(), 
			handles[Store::UrlFrontierItemStore::HostCountCF].get()));
		for (host_iter->SeekToFirst(); host_iter->Valid(); host_iter->Next())
		{
			auto url = host_iter->key().ToString();
			auto count = Helper::SerializerHelper::DeserializeUInt64(host_iter->value().ToString());
			host[url] = count;
		}

		ASSERT_EQ(2, host["baidu.com"]);
		ASSERT_EQ(1, host["xuhongxu.com"]);
		ASSERT_EQ(0, host.count("google.com"));
	}
}