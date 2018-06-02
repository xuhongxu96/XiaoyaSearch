#include "stdafx.h"
#include "LinkServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

LinkServiceImpl::LinkServiceImpl(LinkStore & store)
	: mStore(store)
{ }

::grpc::Status LinkServiceImpl::SaveLinks(::grpc::ServerContext * context, const ArgSaveLinks * request, Result * response)
{
	mStore.SaveLinks(request->url_file_id(),
		std::vector<Link>(request->links().begin(),
			request->links().end()));
	response->set_is_successful(true);

	return Status::OK;
}

::grpc::Status LinkServiceImpl::ClearLinks(::grpc::ServerContext * context, const ArgId * request, Result * response)
{
	mStore.ClearLinks(request->id());
	response->set_is_successful(true);

	return Status::OK;
}

::grpc::Status LinkServiceImpl::GetLinks(::grpc::ServerContext * context, const ArgUrl * request, ResultWithLinks * response)
{
	auto links = mStore.GetLinks(request->url());
	*response->mutable_links()
		= ::google::protobuf::RepeatedPtrField<Link>(links.begin(), links.end());
	response->set_is_successful(true);

	return Status::OK;
}
