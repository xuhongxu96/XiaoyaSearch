#include "stdafx.h"
#include "InvertedIndexServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

InvertedIndexServiceImpl::InvertedIndexServiceImpl(InvertedIndexStore & store)
	: mStore(store)
{ }

Status InvertedIndexServiceImpl::ClearAndSaveIndicesOf(ServerContext * context,
	const ArgClearAndSaveIndicesOf * request, Result * response)
{
	mStore.ClearAndSaveIndicesOf(request->urlfile_id(),
		request->old_urlfile_id(),
		std::vector<Index>(request->indices().begin(), request->indices().end()));
	response->set_is_successful(true);

	return Status::OK;
}

Status InvertedIndexServiceImpl::GetIndex(ServerContext * context,
	const ArgIndexKey * request, ResultWithIndex * response)
{
	response->set_is_successful(
		mStore.GetIndex(request->urlfile_id(),
			request->word(), *response->mutable_index())
	);

	return Status::OK;
}
