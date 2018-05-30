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
		DbTestHelper::FakeLink(1, "http://www.a.com", "a"),	// 1
		DbTestHelper::FakeLink(1, "http://www.a.com", "b"),	// 2
		DbTestHelper::FakeLink(1, "http://www.a.com", "b"),	// 3
		DbTestHelper::FakeLink(1, "http://www.b.com", "d"),	// 4
		DbTestHelper::FakeLink(1, "http://www.c.com", "e"),	// 5
		DbTestHelper::FakeLink(1, "http://www.d.com", "f"),	// 6
		DbTestHelper::FakeLink(1, "http://www.d.com", "g"),	// 7
		DbTestHelper::FakeLink(1, "http://www.e.com", "h"),	// 8
	};

	{
		LinkStore store(config);
		store.SaveLinks(links);
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<LinkStore>(db, handles);

		std::map<std::string, uint16_t> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto links = SerializeHelper::Deserialize<Links>(iter->value().ToString());
			for (auto item : links.items())
			{
				if (items.count(item.url()) == 0)
				{
					items[item.url()] = 1;
				}
				else
				{
					items[item.url()]++;
				}
			}
		}

		ASSERT_EQ(2, items["http://www.a.com"]);
		ASSERT_EQ(1, items["http://www.b.com"]);
		ASSERT_EQ(1, items["http://www.c.com"]);
		ASSERT_EQ(2, items["http://www.d.com"]);
		ASSERT_EQ(1, items["http://www.e.com"]);
	}
}

TEST(LinkStoreTest, TestSaveLinksOfUpdatedUrlFile)
{
	DbTestHelper::DeleteDB<LinkStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto links1 =
	{
		DbTestHelper::FakeLink(1, "http://www.a.com", "a"),	// 1
		DbTestHelper::FakeLink(1, "http://www.a.com", "b"),	// 2
		DbTestHelper::FakeLink(1, "http://www.a.com", "c"),	// 3
		DbTestHelper::FakeLink(1, "http://www.b.com", "d"),	// 4
		DbTestHelper::FakeLink(1, "http://www.c.com", "e"),	// 5
		DbTestHelper::FakeLink(1, "http://www.d.com", "f"),	// 6
		DbTestHelper::FakeLink(1, "http://www.d.com", "g"),	// 7
		DbTestHelper::FakeLink(1, "http://www.e.com", "h"),	// 8
	};

	auto links2 =
	{
		DbTestHelper::FakeLink(2, "http://www.a.com", "a"),	// 9
		DbTestHelper::FakeLink(2, "http://www.b.com", "b"),	// 10
		DbTestHelper::FakeLink(2, "http://www.b.com", "c"),	// 11
		DbTestHelper::FakeLink(2, "http://www.b.com", "d"),	// 12
		DbTestHelper::FakeLink(2, "http://www.c.com", "e"),	// 13
		DbTestHelper::FakeLink(2, "http://www.c.com", "f"),	// 14
		DbTestHelper::FakeLink(2, "http://www.d.com", "g"),	// 15
		DbTestHelper::FakeLink(2, "http://www.d.com", "h"),	// 16
	};

	{
		LinkStore store(config);
		store.SaveLinks(links1);
		store.SaveLinks(links2);
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<LinkStore>(db, handles);

		std::map<std::string, uint16_t> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto links = SerializeHelper::Deserialize<Links>(iter->value().ToString());
			for (auto item : links.items())
			{
				if (items.count(item.url()) == 0)
				{
					items[item.url()] = 1;
				}
				else
				{
					items[item.url()]++;
				}
			}
		}

		ASSERT_EQ(4, items["http://www.a.com"]);
		ASSERT_EQ(4, items["http://www.b.com"]);
		ASSERT_EQ(3, items["http://www.c.com"]);
		ASSERT_EQ(4, items["http://www.d.com"]);
		ASSERT_EQ(1, items.count("http://www.e.com"));
	}
}

