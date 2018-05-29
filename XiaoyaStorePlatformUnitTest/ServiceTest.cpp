#include "pch.h"

#include "DbTestHelper.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Service;
using namespace rocksdb;

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;


void StartServer(std::promise<void> &exitRequested)
{
	auto config = DbTestHelper::InitStoreConfig();
	UrlFrontierItemStore urlFrontierItemStore(config);
	UrlFileStore urlFileStore(config);
	PostingListStore postingListStore(config);
	LinkStore linkStore(config);
	InvertedIndexStore invertedIndexStore(config);

	std::string server_address("0.0.0.0:50051");
	UrlFrontierItemServiceImpl urlFrontierItemService(urlFrontierItemStore);
	UrlFileServiceImpl urlFileService(urlFileStore);
	PostingListServiceImpl postingListService(postingListStore);
	LinkServiceImpl linkService(linkStore);
	InvertedIndexServiceImpl invertedIndexService(invertedIndexStore);

	ServerBuilder builder;
	builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());

	builder.RegisterService(&urlFrontierItemService);
	builder.RegisterService(&urlFileService);
	builder.RegisterService(&postingListService);
	builder.RegisterService(&linkService);
	builder.RegisterService(&invertedIndexService);

	std::unique_ptr<Server> server(builder.BuildAndStart());

	std::thread serverThread([&]()
	{
		std::cerr << "Server listening on " << server_address << std::endl;
		server->Wait();
	});

	auto f = exitRequested.get_future();
	f.wait();
	server->Shutdown();
	serverThread.join();
}

TEST(ServiceTest, TestUrlFrontierItemService)
{
	DbTestHelper::DeleteDB<UrlFrontierItemStore>();

	std::promise<void> exitRequested;
	std::thread serverThread(StartServer, std::ref(exitRequested));

	std::thread clientThread([]()
	{
		auto channel = grpc::CreateChannel("localhost:50051",
			grpc::InsecureChannelCredentials());
		auto service = UrlFrontierItemService::NewStub(channel);

		{
			grpc::ClientContext context;
			ArgUrls urls;
			urls.add_urls("http://www.a.com");
			Result result;

			service->Init(&context, urls, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ResultWithUrl result;
			service->PopUrl(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.a.com", result.url());
		}

		{
			grpc::ClientContext context;
			ArgUrls urls;
			urls.add_urls("http://www.a.com");
			urls.add_urls("http://www.b.com");
			Result result;

			service->PushUrls(&context, urls, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ResultWithUrl result;
			service->PopUrl(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.b.com", result.url());
		}

		{
			grpc::ClientContext context;
			ArgPushBackUrl arg;
			arg.set_url("http://www.b.com");
			arg.set_update_interval(DateTimeHelper::FromDays(1));
			arg.set_failed(false);
			Result result;

			service->PushBackUrl(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ArgPushBackUrl arg;
			arg.set_url("http://www.a.com");
			arg.set_update_interval(DateTimeHelper::FromDays(3));
			arg.set_failed(false);
			Result result;

			service->PushBackUrl(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ResultWithUrl result;
			service->PopUrl(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.b.com", result.url());
		}

		{
			grpc::ClientContext context;
			ResultWithUrl result;
			service->PopUrl(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.a.com", result.url());
		}

		{
			grpc::ClientContext context;
			ArgUrls urls;
			urls.add_urls("http://www.a.com");
			urls.add_urls("http://www.b.com");
			Result result;

			service->PushUrls(&context, urls, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ResultWithUrl result;
			service->PopUrl(&context, ArgVoid(), &result);

			ASSERT_FALSE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ArgUrl url;
			url.set_url("http://www.a.com");
			Result result;
			service->RemoveUrl(&context, url, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ArgUrl url;
			url.set_url("http://www.b.com");
			Result result;
			service->RemoveUrl(&context, url, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ArgUrls urls;
			urls.add_urls("http://www.a.com");
			Result result;

			service->PushUrls(&context, urls, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;
			ResultWithUrl result;
			service->PopUrl(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.a.com", result.url());
		}
	});

	clientThread.join();
	exitRequested.set_value();
	serverThread.join();
}

TEST(ServiceTest, TestUrlFileService)
{
	DbTestHelper::DeleteDB<UrlFileStore>();

	std::promise<void> exitRequested;
	std::thread serverThread(StartServer, std::ref(exitRequested));

	std::thread clientThread([]()
	{
		auto channel = grpc::CreateChannel("localhost:50051",
			grpc::InsecureChannelCredentials());
		auto service = UrlFileService::NewStub(channel);

		{
			grpc::ClientContext context;

			ArgUrlFile arg;
			arg.mutable_urlfile()->CopyFrom(DbTestHelper::FakeUrlFile("http://www.a.com",
				DateTimeHelper::FromDays(1), "a"));

			ResultWithUrlFileAndOldId result;

			service->SaveUrlFileAndGetOldId(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ(1, result.urlfile().urlfile_id());
			ASSERT_EQ(0, result.old_urlfile_id());
		}

		{
			grpc::ClientContext context;

			ArgUrlFile arg;
			arg.mutable_urlfile()->CopyFrom(DbTestHelper::FakeUrlFile("http://www.b.com",
				DateTimeHelper::FromDays(1), "b"));

			ResultWithUrlFileAndOldId result;

			service->SaveUrlFileAndGetOldId(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ(2, result.urlfile().urlfile_id());
			ASSERT_EQ(0, result.old_urlfile_id());
		}
		
		{
			grpc::ClientContext context;

			ArgUrlFile arg;
			arg.mutable_urlfile()->CopyFrom(DbTestHelper::FakeUrlFile("http://www.a.com",
				DateTimeHelper::FromDays(1), "aa"));

			ResultWithUrlFileAndOldId result;

			service->SaveUrlFileAndGetOldId(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ(3, result.urlfile().urlfile_id());
			ASSERT_EQ(1, result.old_urlfile_id());
		}

		{
			grpc::ClientContext context;
			ResultWithCount result;

			service->GetCount(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ(3, result.count());
		}

		{
			grpc::ClientContext context;
			ArgId id;
			id.set_id(3);

			ResultWithUrlFile result;

			service->GetUrlFileById(&context, id, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.a.com", result.urlfile().url());
			ASSERT_EQ("aa", result.urlfile().file_hash());
		}

		{
			grpc::ClientContext context;

			ArgUrl url;
			url.set_url("http://www.a.com");

			ResultWithUrlFile result;

			service->GetUrlFileByUrl(&context, url, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ(3, result.urlfile().urlfile_id());
			ASSERT_EQ("aa", result.urlfile().file_hash());
		}

		{
			grpc::ClientContext context;

			ArgHash hash;
			hash.set_hash("b");

			ResultWithUrlFiles result;

			service->GetUrlFilesByHash(&context, hash, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ(1, result.urlfiles_size());
			ASSERT_EQ(2, result.urlfiles().Get(0).urlfile_id());
			ASSERT_EQ("http://www.b.com", result.urlfiles().Get(0).url());
		}

		{
			grpc::ClientContext context;

			ResultWithUrlFile result;

			service->GetForIndex(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.a.com", result.urlfile().url());
		}

		{
			grpc::ClientContext context;

			ResultWithUrlFile result;

			service->GetForIndex(&context, ArgVoid(), &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.b.com", result.urlfile().url());
		}

		{
			grpc::ClientContext context;

			ResultWithUrlFile result;

			service->GetForIndex(&context, ArgVoid(), &result);

			ASSERT_FALSE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgUrl url;
			url.set_url("http://www.a.com");

			Result result;

			service->FinishIndex(&context, url, &result);

			ASSERT_TRUE(result.is_successful());
		}
	});

	clientThread.join();
	exitRequested.set_value();
	serverThread.join();
}

TEST(ServiceTest, TestPostingListService)
{
	DbTestHelper::DeleteDB<PostingListStore>();

	std::promise<void> exitRequested;
	std::thread serverThread(StartServer, std::ref(exitRequested));

	std::thread clientThread([]()
	{
		auto channel = grpc::CreateChannel("localhost:50051",
			grpc::InsecureChannelCredentials());
		auto service = PostingListService::NewStub(channel);

		{
			grpc::ClientContext context;

			ArgPostingList arg;
			arg.mutable_postinglist()->set_word("a");
			arg.mutable_postinglist()->set_word_frequency(1);
			arg.mutable_postinglist()->set_document_frequency(1);
			arg.mutable_postinglist()->set_is_add(true);
			arg.mutable_postinglist()->add_postings(1);
			arg.mutable_postinglist()->add_postings(2);

			Result result;

			service->SavePostingList(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgPostingList arg;
			arg.mutable_postinglist()->set_word("a");
			arg.mutable_postinglist()->set_word_frequency(3);
			arg.mutable_postinglist()->set_document_frequency(2);
			arg.mutable_postinglist()->set_is_add(true);
			arg.mutable_postinglist()->add_postings(3);
			arg.mutable_postinglist()->add_postings(2);

			Result result;

			service->SavePostingList(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgPostingList arg;
			arg.mutable_postinglist()->set_word("a");
			arg.mutable_postinglist()->set_word_frequency(2);
			arg.mutable_postinglist()->set_document_frequency(1);
			arg.mutable_postinglist()->set_is_add(false);
			arg.mutable_postinglist()->add_postings(2);

			Result result;

			service->SavePostingList(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgWord word;
			word.set_word("a");

			ResultWithPostingList result;

			service->GetPostingList(&context, word, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ(2, result.postinglist().word_frequency());
			ASSERT_EQ(2, result.postinglist().document_frequency());

			std::set<uint64_t> postings(result.postinglist().postings().begin(),
				result.postinglist().postings().end());
			ASSERT_EQ(1, postings.count(1));
			ASSERT_EQ(0, postings.count(2));
			ASSERT_EQ(1, postings.count(3));
		}
	});

	clientThread.join();
	exitRequested.set_value();
	serverThread.join();
}

TEST(ServiceTest, TestLinkService)
{
	DbTestHelper::DeleteDB<LinkStore>();

	std::promise<void> exitRequested;
	std::thread serverThread(StartServer, std::ref(exitRequested));

	std::thread clientThread([]()
	{
		auto channel = grpc::CreateChannel("localhost:50051",
			grpc::InsecureChannelCredentials());
		auto service = LinkService::NewStub(channel);

		{
			grpc::ClientContext context;

			ArgSaveLinkOfUrlFile arg;
			arg.set_urlfile_id(1);
			arg.set_old_urlfile_id(0);
			arg.add_links()->CopyFrom(DbTestHelper::FakeLink(1, "http://www.a.com", "a"));

			Result result;

			service->SaveLinksOfUrlFile(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgSaveLinkOfUrlFile arg;
			arg.set_urlfile_id(2);
			arg.set_old_urlfile_id(0);
			arg.add_links()->CopyFrom(DbTestHelper::FakeLink(2, "http://www.a.com", "2a"));

			Result result;

			service->SaveLinksOfUrlFile(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgSaveLinkOfUrlFile arg;
			arg.set_urlfile_id(3);
			arg.set_old_urlfile_id(1);
			arg.add_links()->CopyFrom(DbTestHelper::FakeLink(3, "http://www.a.com", "3a"));
			arg.add_links()->CopyFrom(DbTestHelper::FakeLink(3, "http://www.b.com", "b"));

			Result result;

			service->SaveLinksOfUrlFile(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgId id;
			id.set_id(4);
			
			ResultWithLink result;

			service->GetLinkById(&context, id, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("http://www.b.com", result.link().url());
			ASSERT_EQ("b", result.link().text());
		}

		{
			grpc::ClientContext context;

			ArgUrl url;
			url.set_url("http://www.a.com");
			
			ResultWithLinks result;

			service->GetLinksByUrl(&context, url, &result);

			ASSERT_TRUE(result.is_successful());

			ASSERT_EQ(2, result.links_size());

			std::set<std::string> linkTextSet;

			for (auto link : result.links())
			{
				linkTextSet.insert(link.text());
			}

			ASSERT_EQ(0, linkTextSet.count("a"));
			ASSERT_EQ(1, linkTextSet.count("2a"));
			ASSERT_EQ(1, linkTextSet.count("3a"));
		}
	});

	clientThread.join();
	exitRequested.set_value();
	serverThread.join();
}

TEST(ServiceTest, TestInvertedIndexService)
{
	DbTestHelper::DeleteDB<InvertedIndexStore>();

	std::promise<void> exitRequested;
	std::thread serverThread(StartServer, std::ref(exitRequested));

	std::thread clientThread([]()
	{
		auto channel = grpc::CreateChannel("localhost:50051",
			grpc::InsecureChannelCredentials());
		auto service = InvertedIndexService::NewStub(channel);

		{
			grpc::ClientContext context;

			ArgClearAndSaveIndicesOf arg;
			arg.set_urlfile_id(1);
			arg.set_old_urlfile_id(0);
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(1, "a"));
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(1, "b"));
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(1, "c"));

			Result result;

			service->ClearAndSaveIndicesOf(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgClearAndSaveIndicesOf arg;
			arg.set_urlfile_id(2);
			arg.set_old_urlfile_id(0);
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(2, "a"));
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(2, "b"));
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(2, "c"));

			Result result;

			service->ClearAndSaveIndicesOf(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}
		
		{
			grpc::ClientContext context;

			ArgClearAndSaveIndicesOf arg;
			arg.set_urlfile_id(3);
			arg.set_old_urlfile_id(1);
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(3, "b"));
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(3, "c"));
			arg.add_indices()->CopyFrom(DbTestHelper::FakeIndex(3, "d"));

			Result result;

			service->ClearAndSaveIndicesOf(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
		}

		{
			grpc::ClientContext context;

			ArgIndexKey arg;
			arg.set_urlfile_id(3);
			arg.set_word("b");

			ResultWithIndex result;

			service->GetIndex(&context, arg, &result);

			ASSERT_TRUE(result.is_successful());
			ASSERT_EQ("b", result.index().key().word());
			ASSERT_EQ(3, result.index().key().urlfile_id());
		}
	});

	clientThread.join();
	exitRequested.set_value();
	serverThread.join();
}
