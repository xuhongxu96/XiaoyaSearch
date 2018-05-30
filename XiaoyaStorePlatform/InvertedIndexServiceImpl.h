#pragma once
#include "stdafx.h"
#include "InvertedIndexStore.h"

namespace XiaoyaStore
{
	namespace Service
	{
		class InvertedIndexServiceImpl final
			: public InvertedIndexService::Service
		{
			Store::InvertedIndexStore &mStore;
		public:
			InvertedIndexServiceImpl(Store::InvertedIndexStore &store);
			::grpc::Status ClearIndices(::grpc::ServerContext* context, const ArgId* request, Result* response) override;
			::grpc::Status SaveIndices(::grpc::ServerContext* context, const ArgSaveIndices* request, Result* response) override;
			::grpc::Status GetIndex(::grpc::ServerContext* context, const ArgIndexKey* request, ResultWithIndex* response) override;
		};
	}
}
