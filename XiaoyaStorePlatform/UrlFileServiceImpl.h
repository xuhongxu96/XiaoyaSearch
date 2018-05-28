#pragma once
#include "stdafx.h"
#include "UrlFileStore.h"

namespace XiaoyaStore
{
	namespace Service
	{
		class UrlFileServiceImpl final
			: public UrlFileService::Service
		{
			Store::UrlFileStore &mStore;
		public:
			UrlFileServiceImpl(Store::UrlFileStore &store);

			::grpc::Status GetUrlFileById(::grpc::ServerContext* context, const ArgId* request, ResultWithUrlFile* response) override;
			::grpc::Status GetUrlFileByUrl(::grpc::ServerContext* context, const ArgUrl* request, ResultWithUrlFile* response) override;
			::grpc::Status GetUrlFilesByHash(::grpc::ServerContext* context, const ArgHash* request, ResultWithUrlFiles* response) override;
			::grpc::Status SaveUrlFileAndGetOldId(::grpc::ServerContext* context, const ArgUrlFile* request, ResultWithUrlFileAndOldId* response) override;
			::grpc::Status GetCount(::grpc::ServerContext* context, const ArgVoid* request, ResultWithCount* response) override;
			::grpc::Status GetForIndex(::grpc::ServerContext* context, const ArgVoid* request, ResultWithUrlFile* response) override;
			::grpc::Status FinishIndex(::grpc::ServerContext* context, const ArgUrl* request, Result* response) override;
		};
	}
}
