#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace rocksdb;

UrlFile FakeUrlFile(const std::string &url,
	const uint64_t updateInterval,
	const std::string &hash = "a")
{
	UrlFile urlFile;
	urlFile.Charset = "UTF-8";
	urlFile.Content = urlFile.TextContent = "Hello, world!";
	urlFile.FileHash = hash;
	urlFile.FilePath = "a";
	urlFile.HeaderCount = 1;
	urlFile.HeaderTotalLength = 1;
	urlFile.InLinkCount = 1;
	urlFile.InLinkTotalLength = 1;
	urlFile.MimeType = "text/html";
	urlFile.PageRank = 0;
	urlFile.PublishDate = DateTimeHelper::Now();
	urlFile.Title = "a";
	urlFile.Url = url;
	urlFile.UpdateInterval = updateInterval;

	return urlFile;
}

TEST(UrlFileStoreTest, TestSaveNew)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto urlFile = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));

	{
		UrlFileStore store(config);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile));
		ASSERT_EQ(1, urlFile.UrlFileId);
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
			ASSERT_EQ(item, urlFile);
		}
	}
}

TEST(UrlFileStoreTest, TestGetUrlFileById)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto urlFile1 = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));
	auto urlFile2 = FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2));

	{
		UrlFileStore store(config);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.UrlFileId);
	}

	{
		UrlFileStore store(config);
		UrlFile getResult;

		ASSERT_TRUE(store.GetUrlFile(1, getResult));
		ASSERT_EQ(urlFile1, getResult);

		ASSERT_TRUE(store.GetUrlFile(2, getResult));
		ASSERT_EQ(urlFile2, getResult);
	}
}

TEST(UrlFileStoreTest, TestGetUrlFileByUrl)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	auto urlFile1 = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));
	auto urlFile2 = FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2));

	{
		UrlFileStore store(config);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.UrlFileId);
	}

	{
		UrlFileStore store(config);
		UrlFile getResult;

		ASSERT_TRUE(store.GetUrlFile("http://www.a.com", getResult));
		ASSERT_EQ(urlFile1, getResult);

		ASSERT_TRUE(store.GetUrlFile("http://www.b.com", getResult));
		ASSERT_EQ(urlFile2, getResult);
	}
}

TEST(UrlFileStoreTest, TestGetUrlFilesByHash)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);

		auto urlFile1 = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile2 = FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile3 = FakeUrlFile("http://www.c.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile3));
		ASSERT_EQ(3, urlFile3.UrlFileId);
	}

	{
		UrlFileStore store(config);

		auto urlFiles = store.GetUrlFilesByHash("a");

		std::sort(urlFiles.begin(), urlFiles.end(),
			[=](const UrlFile &a, const UrlFile &b)
		{
			return a.Url < b.Url;
		});

		ASSERT_EQ(2, urlFiles.size());
		ASSERT_EQ("http://www.a.com", urlFiles[0].Url);
		ASSERT_EQ("http://www.b.com", urlFiles[1].Url);

		urlFiles = store.GetUrlFilesByHash("b");

		ASSERT_EQ(1, urlFiles.size());
		ASSERT_EQ("http://www.c.com", urlFiles[0].Url);
	}
}

TEST(UrlFileStoreTest, TestSaveAndGetOldId)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);

		auto urlFile = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2));

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile));
		ASSERT_EQ(1, urlFile.UrlFileId);
	}

	{
		UrlFileStore store(config);

		auto urlFile = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(1));

		ASSERT_EQ(1, store.SaveUrlFileAndGetOldId(urlFile));
		ASSERT_EQ(2, urlFile.UrlFileId);
	}
}

TEST(UrlFileStoreTest, TestGetCount)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	auto config = DbTestHelper::InitStoreConfig();

	{
		UrlFileStore store(config);

		auto urlFile1 = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile2 = FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile3 = FakeUrlFile("http://www.c.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile3));
		ASSERT_EQ(3, urlFile3.UrlFileId);
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

		auto urlFile1 = FakeUrlFile("http://www.a.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile2 = FakeUrlFile("http://www.b.com", DateTimeHelper::FromDays(2), "a");
		auto urlFile3 = FakeUrlFile("http://www.c.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile1));
		ASSERT_EQ(1, urlFile1.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile2));
		ASSERT_EQ(2, urlFile2.UrlFileId);

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile3));
		ASSERT_EQ(3, urlFile3.UrlFileId);
	}

	{
		UrlFileStore store(config);

		UrlFile urlFile;

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.a.com", urlFile.Url);

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.b.com", urlFile.Url);

		store.FinishIndex(urlFile.Url);

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.c.com", urlFile.Url);

		ASSERT_FALSE(store.GetForIndex(urlFile));

		auto urlFile4 = FakeUrlFile("http://www.d.com", DateTimeHelper::FromDays(2), "b");

		ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile4));
		ASSERT_EQ(4, urlFile4.UrlFileId);

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.d.com", urlFile.Url);
	}

	{
		UrlFileStore store(config);

		UrlFile urlFile;

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.a.com", urlFile.Url);

		store.FinishIndex(urlFile.Url);

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.c.com", urlFile.Url);

		store.FinishIndex(urlFile.Url);

		ASSERT_TRUE(store.GetForIndex(urlFile));
		ASSERT_EQ("http://www.d.com", urlFile.Url);

		store.FinishIndex(urlFile.Url);

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
			auto urlFile = FakeUrlFile("http://www.a" + std::to_string(i) + ".com",
				DateTimeHelper::FromDays(2), std::to_string(i));
			ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile));
			ASSERT_EQ(i, urlFile.UrlFileId);
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
			auto urlFile = FakeUrlFile("http://www.a" + std::to_string(i) + ".com",
				DateTimeHelper::FromDays(2), std::to_string(i));
			ASSERT_EQ(-1, store.SaveUrlFileAndGetOldId(urlFile));
			ASSERT_EQ(i, urlFile.UrlFileId);

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
			ASSERT_EQ(i, urlFile.UrlFileId);

			if (i % 10 == 0)
			{
				std::cerr << i << std::endl;
			}
		}
	}
	auto seconds = (DateTimeHelper::Now() - beginTime) / 1000.0;
	std::cerr << "Used: " << seconds << " seconds" << std::endl;
}