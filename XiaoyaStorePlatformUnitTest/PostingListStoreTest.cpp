#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace rocksdb;

TEST(PostingListStoreTest, TestLoadAndSavePostingList)
{
	DbTestHelper::DeleteDB<PostingListStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		PostingListStore store(config);

		PostingList postingList;

		ASSERT_FALSE(store.GetPostingList("a", postingList));

		postingList.set_is_add(true);
		postingList.set_word("a");
		postingList.set_word_frequency(1);
		postingList.set_document_frequency(1);
		postingList.add_postings(1);
		postingList.add_postings(2);
		postingList.add_postings(3);

		store.SavePostingList(postingList);
		postingList.Clear();

		postingList.set_is_add(true);
		postingList.set_word("a");
		postingList.set_word_frequency(5);
		postingList.set_document_frequency(3);
		postingList.add_postings(1);
		postingList.add_postings(3);
		postingList.add_postings(4);
		postingList.add_postings(5);

		store.SavePostingList(postingList);
		postingList.Clear();
	}

	{
		PostingListStore store(config);

		PostingList postingList;

		ASSERT_TRUE(store.GetPostingList("a", postingList));

		ASSERT_EQ(6, postingList.word_frequency());
		ASSERT_EQ(4, postingList.document_frequency());

		std::set<uint64_t> postingSet(postingList.postings().begin(),
			postingList.postings().end());

		for (uint64_t i = 1; i <= 5; ++i)
		{
			ASSERT_EQ(1, postingSet.count(i));
		}
	}

	{
		PostingListStore store(config);

		PostingList postingList;

		postingList.set_is_add(false);
		postingList.set_word("a");
		postingList.set_word_frequency(2);
		postingList.set_document_frequency(3);
		postingList.add_postings(1);

		store.SavePostingList(postingList);
		postingList.Clear();

		postingList.set_is_add(true);
		postingList.set_word("a");
		postingList.set_word_frequency(5);
		postingList.set_document_frequency(5);
		postingList.add_postings(3);

		store.SavePostingList(postingList);
		postingList.Clear();

		postingList.set_is_add(false);
		postingList.set_word("a");
		postingList.set_word_frequency(1);
		postingList.set_document_frequency(1);
		postingList.add_postings(5);

		store.SavePostingList(postingList);
		postingList.Clear();
	}

	{
		PostingListStore store(config);

		PostingList postingList;

		ASSERT_TRUE(store.GetPostingList("a", postingList));

		ASSERT_EQ(8, postingList.word_frequency());
		ASSERT_EQ(5, postingList.document_frequency());

		std::set<uint64_t> postingSet(postingList.postings().begin(),
			postingList.postings().end());

		for (uint64_t i = 2; i <= 4; ++i)
		{
			ASSERT_EQ(1, postingSet.count(i));
		}
	}
}