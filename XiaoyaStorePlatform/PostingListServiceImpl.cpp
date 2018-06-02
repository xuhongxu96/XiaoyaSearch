#include "stdafx.h"
#include "PostingListServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

PostingListServiceImpl::PostingListServiceImpl(PostingListStore & store)
	:mStore(store)
{ }

Status PostingListServiceImpl::SavePostingLists(ServerContext * context,
	const ArgSavePostingLists * request, Result * response)
{
	mStore.SavePostingLists(request->url_file_id(),
		std::vector<PostingList>(request->posting_list().begin(), request->posting_list().end()));
	response->set_is_successful(true);

	return Status::OK;
}
Status PostingListServiceImpl::ClearPostingLists(ServerContext* context,
	const ArgId* request, Result* response)
{
	mStore.ClearPostingLists(request->id());
	response->set_is_successful(true);

	return Status::OK;
}

Status PostingListServiceImpl::GetPostingList(ServerContext * context,
	const ArgWord * request, ResultWithPostingList * response)
{
	response->set_is_successful(
		mStore.GetPostingList(request->word(), *response->mutable_postinglist())
	);

	return Status::OK;
}
