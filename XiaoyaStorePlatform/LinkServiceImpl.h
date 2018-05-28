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
			::grpc::Status SaveLinksOfUrlFile(::grpc::ServerContext* context, const ArgSaveLinkOfUrlFile* request, Result* response) override;
			::grpc::Status GetLinkById(::grpc::ServerContext* context, const ArgId* request, ResultWithLink* response) override;
			::grpc::Status GetLinksByUrl(::grpc::ServerContext* context, const ArgUrl* request, ResultWithLinks* response) override;
		};
	}
}
