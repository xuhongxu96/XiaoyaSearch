#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace rocksdb;

TEST(LinkStoreTest, TestSaveLinksOfNewUrlFile)
{
	DbTestHelper::DeleteDB<LinkStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto links =
	{
		DbTestHelper::FakeLink(1, "http://www.a.com"),	// 1
		DbTestHelper::FakeLink(1, "http://www.a.com"),	// 2
		DbTestHelper::FakeLink(1, "http://www.a.com"),	// 3
		DbTestHelper::FakeLink(1, "http://www.b.com"),	// 4
		DbTestHelper::FakeLink(1, "http://www.c.com"),	// 5
		DbTestHelper::FakeLink(1, "http://www.d.com"),	// 6
		DbTestHelper::FakeLink(1, "http://www.d.com"),	// 7
		DbTestHelper::FakeLink(1, "http://www.e.com"),	// 8
	};

	{
		LinkStore store(config);
		store.SaveLinksOfUrlFile(1, -1, links);
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<LinkStore>(db, handles);

		std::map<std::string, uint16_t> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<Link>(iter->value().ToString());
			if (items.count(item.url()) == 0)
			{
				items[item.url()] = 1;
			}
			else
			{
				items[item.url()]++;
			}
		}

		ASSERT_EQ(3, items["http://www.a.com"]);
		ASSERT_EQ(1, items["http://www.b.com"]);
		ASSERT_EQ(1, items["http://www.c.com"]);
		ASSERT_EQ(2, items["http://www.d.com"]);
		ASSERT_EQ(1, items["http://www.e.com"]);

		{
			std::string data;
			db->Get(ReadOptions(), handles[LinkStore::UrlIndexCF].get(), "http://www.a.com", &data);
			auto idList = SerializeHelper::Deserialize<IdList>(data);
			ASSERT_EQ(3, idList.ids_size());

			std::set<uint64_t> idSet(idList.ids().begin(), idList.ids().end());

			for (uint64_t i = 1; i <= 3; ++i)
			{
				ASSERT_EQ(1, idSet.count(i));
			}
		}

		{
			std::string data;
			db->Get(ReadOptions(), handles[LinkStore::UrlFileIdIndexCF].get(), 
				SerializeHelper::SerializeUInt64(1), &data);
			auto idList = SerializeHelper::Deserialize<IdList>(data);
			ASSERT_EQ(8, idList.ids_size());

			std::set<uint64_t> idSet(idList.ids().begin(), idList.ids().end());

			for (uint64_t i = 1; i <= 8; ++i)
			{
				ASSERT_EQ(1, idSet.count(i));
			}
		}
	}
}

TEST(LinkStoreTest, TestSaveLinksOfUpdatedUrlFile)
{
	DbTestHelper::DeleteDB<LinkStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto links1 =
	{
		DbTestHelper::FakeLink(1, "http://www.a.com"),	// 1
		DbTestHelper::FakeLink(1, "http://www.a.com"),	// 2
		DbTestHelper::FakeLink(1, "http://www.a.com"),	// 3
		DbTestHelper::FakeLink(1, "http://www.b.com"),	// 4
		DbTestHelper::FakeLink(1, "http://www.c.com"),	// 5
		DbTestHelper::FakeLink(1, "http://www.d.com"),	// 6
		DbTestHelper::FakeLink(1, "http://www.d.com"),	// 7
		DbTestHelper::FakeLink(1, "http://www.e.com"),	// 8
	};

	auto links2 =
	{
		DbTestHelper::FakeLink(2, "http://www.a.com"),	// 9
		DbTestHelper::FakeLink(2, "http://www.b.com"),	// 10
		DbTestHelper::FakeLink(2, "http://www.b.com"),	// 11
		DbTestHelper::FakeLink(2, "http://www.b.com"),	// 12
		DbTestHelper::FakeLink(2, "http://www.c.com"),	// 13
		DbTestHelper::FakeLink(2, "http://www.c.com"),	// 14
		DbTestHelper::FakeLink(2, "http://www.d.com"),	// 15
		DbTestHelper::FakeLink(2, "http://www.d.com"),	// 16
	};

	{
		LinkStore store(config);
		store.SaveLinksOfUrlFile(1, 0, links1);
		store.SaveLinksOfUrlFile(2, 1, links2);
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<LinkStore>(db, handles);

		std::map<std::string, uint16_t> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<Link>(iter->value().ToString());
			if (items.count(item.url()) == 0)
			{
				items[item.url()] = 1;
			}
			else
			{
				items[item.url()]++;
			}
		}

		ASSERT_EQ(1, items["http://www.a.com"]);
		ASSERT_EQ(3, items["http://www.b.com"]);
		ASSERT_EQ(2, items["http://www.c.com"]);
		ASSERT_EQ(2, items["http://www.d.com"]);
		ASSERT_EQ(0, items.count("http://www.e.com"));

		{
			std::string data;
			db->Get(ReadOptions(), handles[LinkStore::UrlIndexCF].get(), "http://www.a.com", &data);
			auto idList = SerializeHelper::Deserialize<IdList>(data);

			std::set<uint64_t> idSet(idList.ids().begin(), idList.ids().end());

			ASSERT_EQ(1, idList.ids_size());
			ASSERT_EQ(1, idSet.count(9));
		}

		{
			std::string data;
			auto status = db->Get(ReadOptions(), handles[LinkStore::UrlFileIdIndexCF].get(), 
				SerializeHelper::SerializeUInt64(1), &data);
			ASSERT_TRUE(status.IsNotFound());

			db->Get(ReadOptions(), handles[LinkStore::UrlFileIdIndexCF].get(), 
				SerializeHelper::SerializeUInt64(2), &data);
			auto idList = SerializeHelper::Deserialize<IdList>(data);
			ASSERT_EQ(8, idList.ids_size());

			std::set<uint64_t> idSet(idList.ids().begin(), idList.ids().end());
			for (uint64_t i = 9; i <= 16; ++i)
			{
				ASSERT_EQ(1, idSet.count(i));
			}
		}
	}
}

