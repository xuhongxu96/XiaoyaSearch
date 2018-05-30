#include "stdafx.h"
#include "LinkServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

LinkServiceImpl::LinkServiceImpl(LinkStore & store)
	: mStore(store)
{ }

::grpc::Status LinkServiceImpl::SaveLinks(::grpc::ServerContext * context, const ArgLinks * request, Result * response)
{
	mStore.SaveLinks(std::vector<Link>(request->links().begin(),
		request->links().end()));
	response->set_is_successful(true);

	return Status::OK;
}

::grpc::Status LinkServiceImpl::RemoveLinks(::grpc::ServerContext * context, const ArgLinks * request, Result * response)
{
	mStore.RemoveLinks(std::vector<Link>(request->links().begin(),
		request->links().end()));
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
