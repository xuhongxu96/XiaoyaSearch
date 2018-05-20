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

		ASSERT_FALSE(store.LoadPostingList("a", postingList));

		postingList.IsAdd = true;
		postingList.Word = "a";
		postingList.WordFrequency = 1;
		postingList.DocumentFrequency = 1;
		postingList.Postings = { 1, 2, 3 };
		
		store.SavePostingList(postingList);

		postingList.IsAdd = true;
		postingList.Word = "a";
		postingList.WordFrequency = 5;
		postingList.DocumentFrequency = 3;
		postingList.Postings = { 1, 3, 4, 5 };

		store.SavePostingList(postingList);
	}

	{
		PostingListStore store(config);

		PostingList postingList;

		ASSERT_TRUE(store.LoadPostingList("a", postingList));

		ASSERT_EQ(6, postingList.WordFrequency);
		ASSERT_EQ(4, postingList.DocumentFrequency);
		
		for (uint64_t i = 1; i <= 5; ++i)
		{
			ASSERT_EQ(1, postingList.Postings.count(i));
		}
	}

	{
		PostingListStore store(config);

		PostingList postingList;

		postingList.IsAdd = false;
		postingList.Word = "a";
		postingList.WordFrequency = -2;
		postingList.DocumentFrequency = -3;
		postingList.Postings = { 1 };

		store.SavePostingList(postingList);

		postingList.IsAdd = true;
		postingList.Word = "a";
		postingList.WordFrequency = 5;
		postingList.DocumentFrequency = 5;
		postingList.Postings = { 3 };

		store.SavePostingList(postingList);

		postingList.IsAdd = false;
		postingList.Word = "a";
		postingList.WordFrequency = -1;
		postingList.DocumentFrequency = -1;
		postingList.Postings = {  5 };

		store.SavePostingList(postingList);
	}

	{
		PostingListStore store(config);

		PostingList postingList;

		ASSERT_TRUE(store.LoadPostingList("a", postingList));

		ASSERT_EQ(8, postingList.WordFrequency);
		ASSERT_EQ(5, postingList.DocumentFrequency);

		for (uint64_t i = 2; i <= 4; ++i)
		{
			ASSERT_EQ(1, postingList.Postings.count(i));
		}
	}
}