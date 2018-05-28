#include "stdafx.h"
#include "PostingListServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

PostingListServiceImpl::PostingListServiceImpl(PostingListStore & store)
	:mStore(store)
{ }

Status PostingListServiceImpl::SavePostingList(ServerContext * context,
	const ArgPostingList * request, Result * response)
{
	mStore.SavePostingList(request->postinglist());
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
