#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace rocksdb;

TEST(InvertedIndexStoreTest, TestSaveNew)
{
	DbTestHelper::DeleteDB<InvertedIndexStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto indices =
	{
		DbTestHelper::FakeIndex(1, "a"),	// 1
		DbTestHelper::FakeIndex(1, "b"),	// 2
		DbTestHelper::FakeIndex(1, "c"),	// 3
		DbTestHelper::FakeIndex(1, "d"),	// 4
	};

	{
		InvertedIndexStore store(config);
		store.SaveIndices(1, indices);
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<InvertedIndexStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<Index>(iter->value().ToString());
			items.insert(item.key().word());
		}

		ASSERT_EQ(1, items.count("a"));
		ASSERT_EQ(1, items.count("b"));
		ASSERT_EQ(1, items.count("c"));
		ASSERT_EQ(1, items.count("d"));
		ASSERT_EQ(0, items.count("e"));

		std::string data;
		db->Get(ReadOptions(), handles[InvertedIndexStore::UrlFileIdIndexCF].get(),
			SerializeHelper::SerializeUInt64(1), &data);
		auto keys = SerializeHelper::Deserialize<IndexKeys>(data);
		ASSERT_EQ(4, keys.items_size());
	}
}

TEST(InvertedIndexStoreTest, TestSaveUpdatedIndices)
{
	DbTestHelper::DeleteDB<InvertedIndexStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto indices1 =
	{
		DbTestHelper::FakeIndex(1, "a"),	// 1
		DbTestHelper::FakeIndex(1, "b"),	// 2
		DbTestHelper::FakeIndex(1, "c"),	// 3
		DbTestHelper::FakeIndex(1, "d"),	// 4
	};

	auto indices2 =
	{
		DbTestHelper::FakeIndex(2, "e"),	// 5
		DbTestHelper::FakeIndex(2, "f"),	// 6
		DbTestHelper::FakeIndex(2, "g"),	// 7
		DbTestHelper::FakeIndex(2, "h"),	// 8
	};

	{
		InvertedIndexStore store(config);
		store.SaveIndices(1, indices1);
		store.ClearIndices(1);
		store.SaveIndices(2, indices2);
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<InvertedIndexStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<Index>(iter->value().ToString());
			items.insert(item.key().word());
		}

		ASSERT_EQ(0, items.count("a"));
		ASSERT_EQ(0, items.count("b"));
		ASSERT_EQ(0, items.count("c"));
		ASSERT_EQ(0, items.count("d"));
		ASSERT_EQ(1, items.count("e"));
		ASSERT_EQ(1, items.count("f"));
		ASSERT_EQ(1, items.count("g"));
		ASSERT_EQ(1, items.count("e"));

		{
			std::string data;

			auto status = db->Get(ReadOptions(), handles[InvertedIndexStore::UrlFileIdIndexCF].get(),
				SerializeHelper::SerializeUInt64(1), &data);
			ASSERT_TRUE(status.IsNotFound());

			db->Get(ReadOptions(), handles[InvertedIndexStore::UrlFileIdIndexCF].get(),
				SerializeHelper::SerializeUInt64(2), &data);

			auto idList = SerializeHelper::Deserialize<IdList>(data);
			auto keys = SerializeHelper::Deserialize<IndexKeys>(data);
			ASSERT_EQ(4, keys.items_size());
		}
	}
}

TEST(InvertedIndexStoreTest, TestSaveNewAndGetIndex)
{
	DbTestHelper::DeleteDB<InvertedIndexStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto indices =
	{
		DbTestHelper::FakeIndex(1, "a"),	// 1
		DbTestHelper::FakeIndex(1, "b"),	// 2
		DbTestHelper::FakeIndex(1, "c"),	// 3
		DbTestHelper::FakeIndex(1, "d"),	// 4
	};

	{
		InvertedIndexStore store(config);
		store.SaveIndices(1, indices);
		Index result;
		ASSERT_TRUE(store.GetIndex(1, "a", result));
		ASSERT_TRUE(store.GetIndex(1, "c", result));
		ASSERT_FALSE(store.GetIndex(1, "e", result));
	}
}
