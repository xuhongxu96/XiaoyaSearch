#include "stdafx.h"
#include "InvertedIndexServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

InvertedIndexServiceImpl::InvertedIndexServiceImpl(InvertedIndexStore & store)
	: mStore(store)
{ }

::grpc::Status XiaoyaStore::Service::InvertedIndexServiceImpl::ClearIndices(::grpc::ServerContext * context, const ArgId * request, Result * response)
{
	mStore.ClearIndices(request->id());
	response->set_is_successful(true);
	return Status::OK;
}

::grpc::Status XiaoyaStore::Service::InvertedIndexServiceImpl::SaveIndices(::grpc::ServerContext * context, const ArgSaveIndices * request, Result * response)
{
	mStore.SaveIndices(request->url_file_id(),
		std::vector<Index>(request->indices().begin(), request->indices().end()));
	response->set_is_successful(true);

	return Status::OK;
}

Status InvertedIndexServiceImpl::GetIndex(ServerContext * context,
	const ArgIndexKey * request, ResultWithIndex * response)
{
	response->set_is_successful(
		mStore.GetIndex(request->url_file_id(),
			request->word(), *response->mutable_index())
	);

	return Status::OK;
}
