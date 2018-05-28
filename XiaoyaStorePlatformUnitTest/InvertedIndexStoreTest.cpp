#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace rocksdb;

Index FakeIndex(const uint64_t urlFileId,
	const std::string &word)
{
	Index index;

	index.mutable_key()->set_urlfile_id(urlFileId);
	index.mutable_key()->set_word(word);

	index.set_occurences_in_headers(std::rand() % 30);
	index.set_occurences_in_links(std::rand() % 100);
	index.set_occurences_in_title(std::rand() % 10);
	index.set_weight(std::rand() / 100.0);
	index.set_word_frequency(std::rand());

	index.add_positions(1);
	index.add_positions(2);
	index.add_positions(3);

	return index;
}

TEST(InvertedIndexStoreTest, TestSaveNew)
{
	DbTestHelper::DeleteDB<InvertedIndexStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto indices =
	{
		FakeIndex(1, "a"),	// 1
		FakeIndex(1, "b"),	// 2
		FakeIndex(1, "c"),	// 3
		FakeIndex(1, "d"),	// 4
	};

	{
		InvertedIndexStore store(config);
		store.ClearAndSaveIndicesOf(1, 0, indices);
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

		{
			std::string data;
			IndexKey indexKey;
			indexKey.set_urlfile_id(1);
			indexKey.set_word("d");
			db->Get(ReadOptions(), handles[InvertedIndexStore::IndexKeyCF].get(),
				SerializeHelper::Serialize(indexKey), &data);
			ASSERT_EQ(4, SerializeHelper::DeserializeUInt64(data));
		}

		{
			std::string data;
			db->Get(ReadOptions(), handles[InvertedIndexStore::UrlFileIdIndexCF].get(),
				SerializeHelper::SerializeUInt64(1), &data);
			auto idList = SerializeHelper::Deserialize<IdList>(data);
			ASSERT_EQ(4, idList.ids_size());

			std::set<uint64_t> idSet(idList.ids().begin(), idList.ids().end());

			for (uint64_t i = 1; i <= 4; ++i)
			{
				ASSERT_EQ(1, idSet.count(i));
			}
		}
	}
}

TEST(InvertedIndexStoreTest, TestSaveUpdatedIndices)
{
	DbTestHelper::DeleteDB<InvertedIndexStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto indices1 =
	{
		FakeIndex(1, "a"),	// 1
		FakeIndex(1, "b"),	// 2
		FakeIndex(1, "c"),	// 3
		FakeIndex(1, "d"),	// 4
	};

	auto indices2 =
	{
		FakeIndex(2, "e"),	// 5
		FakeIndex(2, "f"),	// 6
		FakeIndex(2, "g"),	// 7
		FakeIndex(2, "h"),	// 8
	};

	{
		InvertedIndexStore store(config);
		store.ClearAndSaveIndicesOf(1, 0, indices1);
		store.ClearAndSaveIndicesOf(2, 1, indices2);
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
			IndexKey indexKey;
			indexKey.set_urlfile_id(1);
			indexKey.set_word("a");
			auto status = db->Get(ReadOptions(), handles[InvertedIndexStore::IndexKeyCF].get(),
				SerializeHelper::Serialize(indexKey), &data);
			ASSERT_TRUE(status.IsNotFound());

			indexKey.set_urlfile_id(2);
			indexKey.set_word("e");
			db->Get(ReadOptions(), handles[InvertedIndexStore::IndexKeyCF].get(),
				SerializeHelper::Serialize(indexKey), &data);
			ASSERT_EQ(5, SerializeHelper::DeserializeUInt64(data));
		}

		{
			std::string data;

			auto status = db->Get(ReadOptions(), handles[InvertedIndexStore::UrlFileIdIndexCF].get(),
				SerializeHelper::SerializeUInt64(1), &data);
			ASSERT_TRUE(status.IsNotFound());

			db->Get(ReadOptions(), handles[InvertedIndexStore::UrlFileIdIndexCF].get(),
				SerializeHelper::SerializeUInt64(2), &data);

			auto idList = SerializeHelper::Deserialize<IdList>(data);
			ASSERT_EQ(4, idList.ids_size());

			std::set<uint64_t> idSet(idList.ids().begin(), idList.ids().end());

			for (uint64_t i = 5; i <= 8; ++i)
			{
				ASSERT_EQ(1, idSet.count(i));
			}
		}
	}
}