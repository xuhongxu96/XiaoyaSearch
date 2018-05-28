#pragma once

#include "pch.h"

class DbTestHelper
{
public:
	static const std::string storeDir;

	template <typename T>
	static void OpenDB(std::unique_ptr<rocksdb::DB> &db,
		std::vector<std::unique_ptr<rocksdb::ColumnFamilyHandle>> &handles)
	{
		using namespace XiaoyaStore::Store;
		using namespace XiaoyaStore::Exception;
		// Unmanaged pointers to get values from API call
		rocksdb::DB* db_ptr;
		std::vector<rocksdb::ColumnFamilyHandle*> handle_ptrs;

		auto cfd = std::is_base_of<CounterBaseStore, T>::value ?
			CounterBaseStore::AddIdColumnFamilyDescriptor(T::GetColumnFamilyDescriptors())
			: T::GetColumnFamilyDescriptors();

		auto status = DB::Open(Options(), storeDir + "\\" + T::DbName,
			cfd, &handle_ptrs, &db_ptr);

		if (!status.ok())
		{
			throw StoreException(status, "Failed to open DB: " + T::DbName);
		}

		// Manage pointers using std::unique_ptr
		db.reset(db_ptr);

		handles.resize(handle_ptrs.size());
		for (size_t i = 0; i < handles.size(); ++i)
		{
			handles[i] = std::move(std::unique_ptr<rocksdb::ColumnFamilyHandle>(
				handle_ptrs[i]));
		}
	}

	template <typename T>
	static void DeleteDB()
	{
		namespace fs = boost::filesystem;

		fs::path dbPath(storeDir + "\\" + T::DbName);
		if (fs::exists(dbPath))
		{
			fs::remove_all(dbPath);
		}
	}

	static XiaoyaStore::Config::StoreConfig InitStoreConfig()
	{
		XiaoyaStore::Config::StoreConfig config;
		config.StoreDirectory = storeDir;
		return config;
	}

	static XiaoyaStore::Model::UrlFile FakeUrlFile(const std::string &url,
		const uint64_t updateInterval,
		const std::string &hash = "a")
	{
		XiaoyaStore::Model::UrlFile urlFile;

		urlFile.set_charset("UTF-8");
		urlFile.set_content("Hello, world!");
		urlFile.set_text_content("Hello, world!");
		urlFile.set_file_hash(hash);
		urlFile.set_file_path("a");
		urlFile.set_header_count(1);
		urlFile.set_header_total_length(1);
		urlFile.set_in_link_count(1);
		urlFile.set_in_link_total_length(1);
		urlFile.set_mime_type("text/html");
		urlFile.set_page_rank(0);
		urlFile.set_publish_date(XiaoyaStore::Helper::DateTimeHelper::Now());
		urlFile.set_title("a");
		urlFile.set_url(url);
		urlFile.set_update_interval(updateInterval);

		return urlFile;
	}

	static XiaoyaStore::Model::Link FakeLink(const uint64_t urlFileId,
		const std::string &url = "http://www.a.com",
		const std::string &text = "a")
	{
		XiaoyaStore::Model::Link link;
		link.set_urlfile_id(urlFileId);
		link.set_text(text);
		link.set_url(url);
		return link;
	}

	static XiaoyaStore::Model::Index FakeIndex(const uint64_t urlFileId,
		const std::string &word)
	{
		XiaoyaStore::Model::Index index;

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
};