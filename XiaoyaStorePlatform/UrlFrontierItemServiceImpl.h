#pragma once
#include "stdafx.h"
#include "UrlFrontierItemStore.h"

namespace XiaoyaStore
{
	namespace Service
	{
		class UrlFrontierItemServiceImpl final
			: public UrlFrontierItemService::Service
		{
			Store::UrlFrontierItemStore &mStore;
		public:
			UrlFrontierItemServiceImpl(Store::UrlFrontierItemStore &store);

			::grpc::Status Init(::grpc::ServerContext* context, const ArgUrls* request, Result* response) override;
			::grpc::Status PushUrls(::grpc::ServerContext* context, const ArgUrls* request, Result* response) override;
			::grpc::Status PushBackUrl(::grpc::ServerContext* context, const ArgPushBackUrl* request, Result* response) override;
			::grpc::Status PopUrl(::grpc::ServerContext* context, const ArgVoid* request, ResultWithUrl* response) override;
			::grpc::Status RemoveUrl(::grpc::ServerContext* context, const ArgUrl* request, Result* response) override;
			::grpc::Status GetHostCount(::grpc::ServerContext* context, const ArgHost* request, ResultWithCount* response) override;
		};
	}
}