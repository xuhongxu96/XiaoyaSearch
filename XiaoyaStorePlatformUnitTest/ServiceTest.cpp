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

std::promise<void> exitRequested;

void StartServer()
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

	std::thread serverThread(StartServer);

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