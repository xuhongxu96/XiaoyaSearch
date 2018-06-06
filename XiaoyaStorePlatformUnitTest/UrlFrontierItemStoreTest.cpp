#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace rocksdb;

TEST(UrlFrontierItemStoreTest, TestInit)
{
	DbTestHelper::DeleteDB<UrlFrontierItemStore>();

	auto config = DbTestHelper::InitStoreConfig();

	// Init UrlFrontierStore with urls
	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.Init({
			"http://baidu.com",
			"http://baidu.com",
			"http://baidu.com/a",
			"http://xuhongxu.com",
			});
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<UrlFrontierItemStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<UrlFrontierItem>(iter->value().ToString());
			items.insert(item.url());
		}

		ASSERT_EQ(1, items.count("http://baidu.com"));
		ASSERT_EQ(1, items.count("http://baidu.com/a"));
		ASSERT_EQ(1, items.count("http://xuhongxu.com"));
		ASSERT_EQ(0, items.count("http://google.com"));

		std::map<std::string, uint64_t> host;

		std::unique_ptr<Iterator> host_iter(db->NewIterator(ReadOptions(),
			handles[UrlFrontierItemStore::HostCountCF].get()));
		for (host_iter->SeekToFirst(); host_iter->Valid(); host_iter->Next())
		{
			auto url = host_iter->key().ToString();
			auto count = SerializeHelper::DeserializeUInt64(host_iter->value().ToString());
			host[url] = count;
		}

		ASSERT_EQ(2, host["baidu.com"]);
		ASSERT_EQ(1, host["xuhongxu.com"]);
		ASSERT_EQ(0, host.count("google.com"));
	}

	// 2nd Init UrlFrontierStore with urls
	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.Init({
			"http://baidu.com",
			"http://baidu.com/b",
			"https://google.com",
			});
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<UrlFrontierItemStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<UrlFrontierItem>(iter->value().ToString());
			items.insert(item.url());
		}

		ASSERT_EQ(1, items.count("http://baidu.com"));
		ASSERT_EQ(1, items.count("http://baidu.com/a"));
		ASSERT_EQ(1, items.count("http://baidu.com/b"));
		ASSERT_EQ(1, items.count("http://xuhongxu.com"));
		ASSERT_EQ(1, items.count("https://google.com"));
		ASSERT_EQ(0, items.count("http://google.com"));

		std::map<std::string, uint64_t> host;

		std::unique_ptr<Iterator> host_iter(db->NewIterator(ReadOptions(),
			handles[UrlFrontierItemStore::HostCountCF].get()));
		for (host_iter->SeekToFirst(); host_iter->Valid(); host_iter->Next())
		{
			auto url = host_iter->key().ToString();
			auto count = SerializeHelper::DeserializeUInt64(host_iter->value().ToString());
			host[url] = count;
		}

		ASSERT_EQ(3, host["baidu.com"]);
		ASSERT_EQ(1, host["xuhongxu.com"]);
		ASSERT_EQ(1, host.count("google.com"));
	}
}

TEST(UrlFrontierItemStoreTest, TestPushUrls)
{

	DbTestHelper::DeleteDB<UrlFrontierItemStore>();

	auto config = DbTestHelper::InitStoreConfig();

	// Push Urls
	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.PushUrls({
			"http://baidu.com",
			"http://baidu.com",
			"http://baidu.com/a",
			"http://xuhongxu.com",
			});
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<UrlFrontierItemStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<UrlFrontierItem>(iter->value().ToString());
			items.insert(item.url());
		}

		ASSERT_EQ(1, items.count("http://baidu.com"));
		ASSERT_EQ(1, items.count("http://baidu.com/a"));
		ASSERT_EQ(1, items.count("http://xuhongxu.com"));
		ASSERT_EQ(0, items.count("http://google.com"));

		std::map<std::string, uint64_t> host;

		std::unique_ptr<Iterator> host_iter(db->NewIterator(ReadOptions(),
			handles[UrlFrontierItemStore::HostCountCF].get()));
		for (host_iter->SeekToFirst(); host_iter->Valid(); host_iter->Next())
		{
			auto url = host_iter->key().ToString();
			auto count = SerializeHelper::DeserializeUInt64(host_iter->value().ToString());
			host[url] = count;
		}

		ASSERT_EQ(2, host["baidu.com"]);
		ASSERT_EQ(1, host["xuhongxu.com"]);
		ASSERT_EQ(0, host.count("google.com"));
	}

	// 2nd PushUrls
	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.PushUrls({
			"http://baidu.com",
			"http://baidu.com/b",
			"https://google.com",
			});
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<UrlFrontierItemStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<UrlFrontierItem>(iter->value().ToString());
			items.insert(item.url());
		}

		ASSERT_EQ(1, items.count("http://baidu.com"));
		ASSERT_EQ(1, items.count("http://baidu.com/a"));
		ASSERT_EQ(1, items.count("http://baidu.com/b"));
		ASSERT_EQ(1, items.count("http://xuhongxu.com"));
		ASSERT_EQ(1, items.count("https://google.com"));
		ASSERT_EQ(0, items.count("http://google.com"));

		std::map<std::string, uint64_t> host;

		std::unique_ptr<Iterator> host_iter(db->NewIterator(ReadOptions(),
			handles[UrlFrontierItemStore::HostCountCF].get()));
		for (host_iter->SeekToFirst(); host_iter->Valid(); host_iter->Next())
		{
			auto url = host_iter->key().ToString();
			auto count = SerializeHelper::DeserializeUInt64(host_iter->value().ToString());
			host[url] = count;
		}

		ASSERT_EQ(3, host["baidu.com"]);
		ASSERT_EQ(1, host["xuhongxu.com"]);
		ASSERT_EQ(1, host.count("google.com"));
	}
}

TEST(UrlFrontierItemStoreTest, TestPushBackUrl)
{

	DbTestHelper::DeleteDB<UrlFrontierItemStore>();

	auto config = DbTestHelper::InitStoreConfig();

	std::string poppedUrl;

	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.PushUrls({
			"http://baidu.com",
			"http://xuhongxu.com",
			});

		ASSERT_FALSE(store.PushBackUrl("http://baidu.com", DateTimeHelper::FromDays(1)));
		ASSERT_FALSE(store.PushBackUrl("http://google.com", DateTimeHelper::FromDays(1)));

		ASSERT_TRUE(store.PopUrl(poppedUrl));

		ASSERT_TRUE(store.PushBackUrl(poppedUrl, DateTimeHelper::FromDays(1)));
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<UrlFrontierItemStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<UrlFrontierItem>(iter->value().ToString());
			if (item.url() == poppedUrl)
			{
				std::cout << item.url() << std::endl;
				std::cout << item.planned_time() << std::endl;
				std::cout << item.priority() << std::endl;

				ASSERT_GT(item.planned_time(), DateTimeHelper::Now() + DateTimeHelper::FromHours(1));
			}
		}
	}
}

TEST(UrlFrontierItemStoreTest, TestRemoveUrl)
{

	DbTestHelper::DeleteDB<UrlFrontierItemStore>();

	auto config = DbTestHelper::InitStoreConfig();

	std::string poppedUrl;

	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.PushUrls({
			"http://baidu.com",
			"http://xuhongxu.com",
			});

		ASSERT_TRUE(store.PopUrl(poppedUrl));

		store.RemoveUrl(poppedUrl);
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<UrlFrontierItemStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<UrlFrontierItem>(iter->value().ToString());
			ASSERT_NE(poppedUrl, item.url());
		}
	}
}

TEST(UrlFrontierItemStoreTest, TestGetHostCount)
{

	DbTestHelper::DeleteDB<UrlFrontierItemStore>();

	auto config = DbTestHelper::InitStoreConfig();

	// Init UrlFrontierStore with urls
	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.Init({
			"http://baidu.com",
			"http://baidu.com/a",
			"http://xuhongxu.com",
			});

		ASSERT_EQ(2, store.GetHostCount("baidu.com"));
		ASSERT_EQ(1, store.GetHostCount("xuhongxu.com"));
		ASSERT_EQ(0, store.GetHostCount("google.com"));
	}
}

TEST(UrlFrontierItemStoreTest, TestPopUrl)
{

	DbTestHelper::DeleteDB<UrlFrontierItemStore>();

	auto config = DbTestHelper::InitStoreConfig();

	// Init UrlFrontierStore with urls
	{
		UrlFrontierItemStore store(config);
		store.ReloadUrlFrontierItems();
		store.PushUrls({	// PlannedTime,	Priority
"http://baidu.com",			// t0,			0 + 0 * 10 = 0
"http://baidu.com/a",		// t0,			1 + 1 * 10 = 11
"http://xuhongxu.com/b",	// t0,			0 + 1 * 10 = 10
			});

		std::string url;

		store.PopUrl(url);
		ASSERT_EQ("http://baidu.com", url);

		/*
		"http://baidu.com/a",		// t0,		1 + 1 * 10 = 11
		"http://xuhongxu.com/b",	// t0,		0 + 1 * 10 = 10
		*/

		store.PopUrl(url);
		ASSERT_EQ("http://xuhongxu.com/b", url);

		/*
		"http://baidu.com/a",		// t0,		1 + 1 * 10 = 11
		*/

		// Wait 2 seconds
		std::this_thread::sleep_for(std::chrono::seconds(2));

		store.PushBackUrl("http://baidu.com", 0);	// t1,		1 + 0 * 10 = 1
		store.PushUrls({
			"http://xuhongxu.com",					// t1,		0 + 0 * 10 = 0
			});

		/*
		"http://baidu.com/a",		// t0,		1 + 1 * 10 = 11
		"http://baidu.com",			// t1,		1 + 0 * 10 = 1
		"http://xuhongxu.com",		// t1,		0 + 0 * 10 = 0
		*/

		store.PopUrl(url);
		ASSERT_EQ("http://baidu.com/a", url);

		/*
		"http://baidu.com",			// t1,		1 + 0 * 10 = 1
		"http://xuhongxu.com",		// t1,		0 + 0 * 10 = 0
		*/

		store.PopUrl(url);
		ASSERT_EQ("http://xuhongxu.com", url);

		store.PopUrl(url);
		ASSERT_EQ("http://baidu.com", url);
	}
}
