#include "stdafx.h"
#include "LinkServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

LinkServiceImpl::LinkServiceImpl(LinkStore & store)
	: mStore(store)
{ }

Status LinkServiceImpl::SaveLinksOfUrlFile(ServerContext * context,
	const ArgSaveLinkOfUrlFile * request, Result * response)
{
	mStore.SaveLinksOfUrlFile(request->urlfile_id(),
		request->old_urlfile_id(),
		std::vector<Link>(request->links().begin(), request->links().end()));
	response->set_is_successful(true);

	return Status::OK;
}

Status LinkServiceImpl::GetLinkById(ServerContext * context,
	const ArgId * request, ResultWithLink * response)
{
	response->set_is_successful(
		mStore.GetLink(request->id(), *response->mutable_link())
	);

	return Status::OK;
}

Status LinkServiceImpl::GetLinksByUrl(ServerContext * context,
	const ArgUrl * request, ResultWithLinks * response)
{
	auto links = mStore.GetLinksByUrl(request->url());
	*response->mutable_links()
		= ::google::protobuf::RepeatedPtrField<Link>(links.begin(), links.end());
	response->set_is_successful(true);
	
	return Status::OK;
}
