// XiaoyaStorePlatformServer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

using namespace XiaoyaStore;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;
using namespace XiaoyaStore::Helper;
using namespace XiaoyaStore::Service;

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;

int main()
{
	XiaoyaStore::Config::StoreConfig config;
	config.EnableExactPlannedTime = true;
	config.StoreDirectory = boost::filesystem::current_path().string();

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
		std::cout << "Server listening on " << server_address << std::endl;
		server->Wait();
	});

	while (true)
	{
		std::cout << "Input `exit` to Exit." << std::endl;
		std::string cmd;
		std::cin >> cmd;
		if (cmd == "exit")
		{
			server->Shutdown();
			serverThread.join();
			break;
		}
	}

	std::cout << "Server has stopped" << std::endl;
	return 0;
}

