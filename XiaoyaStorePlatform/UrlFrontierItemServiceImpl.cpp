#include "stdafx.h"
#include "UrlFrontierItemServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;

UrlFrontierItemServiceImpl::UrlFrontierItemServiceImpl(UrlFrontierItemStore & store)
	: mStore(store)
{ }

Status UrlFrontierItemServiceImpl::Init(ServerContext * context,
	const ArgUrls * request, Result * response)
{
	mStore.Init(std::vector<std::string>(request->urls().begin(),
		request->urls().end()));
	response->set_is_successful(true);
	return Status::OK;
}

Status UrlFrontierItemServiceImpl::Reload(ServerContext * context,
	const ArgVoid * request, Result * response)
{
	mStore.ReloadUrlFrontierItems();
	response->set_is_successful(true);

	return Status::OK;
}

Status UrlFrontierItemServiceImpl::PushUrls(ServerContext * context,
	const ArgUrls * request, Result * response)
{
	mStore.PushUrls(std::vector<std::string>(request->urls().begin(),
		request->urls().end()));
	response->set_is_successful(true);
	return Status::OK;
}

Status UrlFrontierItemServiceImpl::PushBackUrl(ServerContext * context,
	const ArgPushBackUrl * request, Result * response)
{
	response->set_is_successful(
		mStore.PushBackUrl(request->url(), request->update_interval(), request->failed())
	);

	return Status::OK;
}

Status UrlFrontierItemServiceImpl::PopUrl(ServerContext * context,
	const ArgVoid * request, ResultWithUrl * response)
{
	std::string url;

	response->set_is_successful(mStore.PopUrl(url));
	response->set_url(url);

	return Status::OK;
}

Status UrlFrontierItemServiceImpl::RemoveUrl(ServerContext * context,
	const ArgUrl * request, Result * response)
{
	mStore.RemoveUrl(request->url());
	response->set_is_successful(true);

	return Status::OK;
}

Status UrlFrontierItemServiceImpl::GetHostCount(ServerContext * context,
	const ArgHost * request, ResultWithCount * response)
{
	response->set_count(
		mStore.GetHostCount(request->host())
	);
	response->set_is_successful(true);

	return Status::OK;
}
