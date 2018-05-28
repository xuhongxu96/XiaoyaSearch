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
			::grpc::Status ClearAndSaveIndicesOf(::grpc::ServerContext* context, const ArgClearAndSaveIndicesOf* request, Result* response);
			::grpc::Status GetIndex(::grpc::ServerContext* context, const ArgIndexKey* request, ResultWithIndex* response);
		};
	}
}
