#pragma once
#include "stdafx.h"
#include "PostingListStore.h"

namespace XiaoyaStore
{
	namespace Service
	{
		class PostingListServiceImpl final
			: public PostingListService::Service
		{
			Store::PostingListStore &mStore;
		public:
			PostingListServiceImpl(Store::PostingListStore &store);
			::grpc::Status SavePostingList(::grpc::ServerContext* context, const ArgPostingList* request, Result* response) override;
			::grpc::Status GetPostingList(::grpc::ServerContext* context, const ArgWord* request, ResultWithPostingList* response) override;
		};
	}
}
