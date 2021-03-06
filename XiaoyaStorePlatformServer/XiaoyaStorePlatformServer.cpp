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

	std::promise<void> coutPromise;

	std::thread serverThread([&]()
	{
		std::cout << "Server listening on " << server_address << std::endl;
		coutPromise.set_value();
		server->Wait();
	});

	coutPromise.get_future().wait();

	while (true)
	{
		std::cout << "Input `info` to Output Debug Info." << std::endl;
		std::cout << "Input `exit` to Exit." << std::endl;
		std::string cmd;
		std::cin >> cmd;
		std::transform(cmd.begin(), cmd.end(), cmd.begin(), ::tolower);
		if (cmd == "exit")
		{
			server->Shutdown();
			serverThread.join();
			break;
		}
		else if (cmd == "info")
		{
			std::ofstream fs("debug.txt");
			if (fs.is_open())
			{
				fs << "Now: " << DateTimeHelper::Now() << std::endl;
				fs << "Now / 60000: " << DateTimeHelper::Now() / 60000 << std::endl;
				fs << std::endl;

				fs << "UrlFile Count: " << urlFileStore.GetCount() << std::endl;
				fs << std::endl;

				fs << "Top 2000 UrlFrontierItems" << std::endl;
				fs << std::endl;

				for (auto item : urlFrontierItemStore.PeekTopUrlFrontierItems(3000))
				{
					fs << item.url() << std::endl;
					fs << "PlannedTime: " << item.planned_time() << std::endl;
					fs << "PlannedTime / 60000: " << item.planned_time() / 60000 << std::endl;
					fs << "Priority: " << item.priority() << std::endl;
					fs << "FailedTimes: " << item.failed_times() << std::endl;
					fs << "UpdatedAt: " << item.updated_at() << std::endl;
					fs << std::endl;
				}

				fs << "Url Hosts" << std::endl;
				fs << std::endl;

				for (auto item : urlFrontierItemStore.GetHosts())
				{
					if (item.first > 0)
					{
						fs << item.second << ": " << item.first << std::endl;
					}
				}

				fs.close();
			}
		}
	}

	std::cout << "Server has stopped" << std::endl;
	return 0;
}

