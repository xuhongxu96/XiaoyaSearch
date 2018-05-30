#pragma once
#include "stdafx.h"
#include "LinkStore.h"

namespace XiaoyaStore
{
	namespace Service
	{
		class LinkServiceImpl final
			: public LinkService::Service
		{
			Store::LinkStore &mStore;
		public:
			LinkServiceImpl(Store::LinkStore &store);
			::grpc::Status SaveLinks(::grpc::ServerContext* context, const ArgLinks* request, Result* response) override;
			::grpc::Status RemoveLinks(::grpc::ServerContext* context, const ArgLinks* request, Result* response) override;
			::grpc::Status GetLinks(::grpc::ServerContext* context, const ArgUrl* request, ResultWithLinks* response) override;
		};
	}
}
