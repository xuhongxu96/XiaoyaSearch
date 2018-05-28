#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace rocksdb;

TEST(UrlFileStoreTest, TestSaveNew)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto urlFile = DbTestHelper::DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));

	{
		UrlFileStore store(config);

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile));
		ASSERT_EQ(1, urlFile.urlfile_id());
	}

	{
		std::unique_ptr<DB> db;
		std::vector<std::unique_ptr<ColumnFamilyHandle>> handles;

		DbTestHelper::OpenDB<UrlFileStore>(db, handles);

		std::set<std::string> items;

		std::unique_ptr<Iterator> iter(db->NewIterator(ReadOptions()));
		for (iter->SeekToFirst(); iter->Valid(); iter->Next())
		{
			auto item = SerializeHelper::Deserialize<UrlFile>(iter->value().ToString());
			ASSERT_EQ(item.SerializeAsString(), urlFile.SerializeAsString());
		}
	}
}

TEST(UrlFileStoreTest, TestGetUrlFileById)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto urlFile1 = DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));
	auto urlFile2 = DbTestHelper::FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2));

	{
		UrlFileStore store(config);

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.urlfile_id());
	}

	{
		UrlFileStore store(config);
		UrlFile getResult;

		ASSERT_TRUE(store.GetUrlFile(1, getResult));
		ASSERT_EQ(urlFile1.SerializeAsString(), getResult.SerializeAsString());

		ASSERT_TRUE(store.GetUrlFile(2, getResult));
		ASSERT_EQ(urlFile2.SerializeAsString(), getResult.SerializeAsString());
	}
}

TEST(UrlFileStoreTest, TestGetUrlFileByUrl)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto urlFile1 = DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));
	auto urlFile2 = DbTestHelper::FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2));

	{
		UrlFileStore store(config);

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.urlfile_id());
	}

	{
		UrlFileStore store(config);
		UrlFile getResult;

		ASSERT_TRUE(store.GetUrlFile("http://www.a.com", getResult));
		ASSERT_EQ(urlFile1.SerializeAsString(), getResult.SerializeAsString());

		ASSERT_TRUE(store.GetUrlFile("http://www.b.com", getResult));
		ASSERT_EQ(urlFile2.SerializeAsString(), getResult.SerializeAsString());
	}
}

TEST(UrlFileStoreTest, TestGetUrlFilesByHash)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);

		auto urlFile1 = DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile2 = DbTestHelper::FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile3 = DbTestHelper::FakeUrlFile("http://www.c.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile3));
		ASSERT_EQ(3, urlFile3.urlfile_id());
	}

	{
		UrlFileStore store(config);

		auto urlFiles = store.GetUrlFilesByHash("a");

		std::sort(urlFiles.begin(), urlFiles.end(),
			[=](const UrlFile &a, const UrlFile &b)
		{
			return a.url() < b.url();
		});

		ASSERT_EQ(2, urlFiles.size());
		ASSERT_EQ("http://www.a.com", urlFiles[0].url());
		ASSERT_EQ("http://www.b.com", urlFiles[1].url());

		urlFiles = store.GetUrlFilesByHash("b");

		ASSERT_EQ(1, urlFiles.size());
		ASSERT_EQ("http://www.c.com", urlFiles[0].url());
	}
}

TEST(UrlFileStoreTest, TestSaveAndGetOldId)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);

		auto urlFile = DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile));
		ASSERT_EQ(1, urlFile.urlfile_id());
	}

	{
		UrlFileStore store(config);

		auto urlFile = DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(1));

		ASSERT_EQ(1, store.SaveUrlFileAndGetOldId(urlFile));
		ASSERT_EQ(2, urlFile.urlfile_id());
	}
}

TEST(UrlFileStoreTest, TestGetCount)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);

		auto urlFile1 = DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile2 = DbTestHelper::FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile3 = DbTestHelper::FakeUrlFile("http://www.c.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile3));
		ASSERT_EQ(3, urlFile3.urlfile_id());
	}

	{
		UrlFileStore store(config);

		ASSERT_EQ(3, store.GetCount());
	}
}

TEST(UrlFileStoreTest, TestGetForIndexAndFinishIndex)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);

		auto urlFile1 = DbTestHelper::FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile2 = DbTestHelper::FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile3 = DbTestHelper::FakeUrlFile("http://www.c.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.urlfile_id());

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile3));
		ASSERT_EQ(3, urlFile3.urlfile_id());
	}

	{
		UrlFileStore store(config);

		UrlFile urlFile;

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.a.com", urlFile.url());

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.b.com", urlFile.url());

		store.FinishIndex(urlFile.url());

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.c.com", urlFile.url());

		ASSERT_FALSE(store.GetForIndex(urlFile));

		auto urlFile4 = DbTestHelper::FakeUrlFile("http://www.d.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile4));
		ASSERT_EQ(4, urlFile4.urlfile_id());

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.d.com", urlFile.url());
	}

	{
		UrlFileStore store(config);

		UrlFile urlFile;

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.a.com", urlFile.url());

		store.FinishIndex(urlFile.url());

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.c.com", urlFile.url());

		store.FinishIndex(urlFile.url());

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.d.com", urlFile.url());

		store.FinishIndex(urlFile.url());

		ASSERT_FALSE(store.GetForIndex(urlFile));
	}

	{
		UrlFileStore store(config);

		UrlFile urlFile;

		ASSERT_FALSE(store.GetForIndex(urlFile));
	}
}

TEST(UrlFileStoreTest, StressTestSave)
{
	const int testCount = 5000;

	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto beginTime = DateTimeHelper::Now();
	{
		UrlFileStore store(config);
		for (int i = 1; i <= testCount; ++i)
		{
			auto urlFile = DbTestHelper::FakeUrlFile("http://www.a" + std::to_string(i) + ".com",
				DateTimeHelper::FromDays(2), std::to_string(i));
			ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile));
			ASSERT_EQ(i, urlFile.urlfile_id());
			if (i % 10 == 0)
			{
				std::cerr << i << std::endl;
			}
		}
	}
	auto seconds = (DateTimeHelper::Now() - beginTime) / 1000.0;
	std::cerr << "Used: " << seconds << " seconds" << std::endl;
}

TEST(UrlFileStoreTest, StressTestLoad)
{
	const int testCount = 5000;

	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);
		for (int i = 1; i <= testCount; ++i)
		{
			auto urlFile = DbTestHelper::FakeUrlFile("http://www.a" + std::to_string(i) + ".com",
				DateTimeHelper::FromDays(2), std::to_string(i));
			ASSERT_EQ(0, store.SaveUrlFileAndGetOldId(urlFile));
			ASSERT_EQ(i, urlFile.urlfile_id());

			if (i % 10 == 0)
			{
				std::cerr << i << std::endl;
			}
		}
	}

	std::cerr << "Saved All. Start Testing Load..." << std::endl;

	auto beginTime = DateTimeHelper::Now();
	{
		UrlFileStore store(config);
		UrlFile urlFile;
		for (int i = 1; i <= testCount; ++i)
		{
			auto result = store.GetUrlFile(i, urlFile);
			ASSERT_TRUE(result);
			ASSERT_EQ(i, urlFile.urlfile_id());

			if (i % 10 == 0)
			{
				std::cerr << i << std::endl;
			}
		}
	}
	auto seconds = (DateTimeHelper::Now() - beginTime) / 1000.0;
	std::cerr << "Used: " << seconds << " seconds" << std::endl;
}