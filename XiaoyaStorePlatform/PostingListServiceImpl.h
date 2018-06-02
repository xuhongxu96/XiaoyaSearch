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
			::grpc::Status SavePostingLists(::grpc::ServerContext* context, const ArgSavePostingLists* request, Result* response) override;
			::grpc::Status ClearPostingLists(::grpc::ServerContext* context, const ArgId* request, Result* response) override;
			::grpc::Status GetPostingList(::grpc::ServerContext* context, const ArgWord* request, ResultWithPostingList* response) override;
		};
	}
}
