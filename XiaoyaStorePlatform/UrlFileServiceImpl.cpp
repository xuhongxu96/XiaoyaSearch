#include "stdafx.h"
#include "UrlFileServiceImpl.h"

using namespace grpc;
using namespace XiaoyaStore::Service;
using namespace XiaoyaStore::Store;
using namespace XiaoyaStore::Model;

UrlFileServiceImpl::UrlFileServiceImpl(UrlFileStore & store)
	: mStore(store)
{ }

Status UrlFileServiceImpl::GetUrlFileById(ServerContext * context,
	const ArgId * request, ResultWithUrlFile * response)
{
	response->set_is_successful(
		mStore.GetUrlFile(request->id(), *response->mutable_urlfile())
	);
	return Status::OK;
}

Status UrlFileServiceImpl::GetUrlFileByUrl(ServerContext * context,
	const ArgUrl * request, ResultWithUrlFile * response)
{
	response->set_is_successful(
		mStore.GetUrlFile(request->url(), *response->mutable_urlfile())
	);
	return Status::OK;
}

Status UrlFileServiceImpl::GetUrlFilesByHash(ServerContext * context,
	const ArgHash * request, ResultWithUrlFiles * response)
{
	auto urlFiles = mStore.GetUrlFilesByHash(request->hash());

	*response->mutable_urlfiles() 
		= ::google::protobuf::RepeatedPtrField<UrlFile>(urlFiles.begin(), urlFiles.end());
	response->set_is_successful(true);

	return Status::OK;
}

Status UrlFileServiceImpl::SaveUrlFileAndGetOldId(ServerContext * context,
	const ArgUrlFile * request, ResultWithUrlFileAndOldId * response)
{
	response->mutable_urlfile()->CopyFrom(request->urlfile());
	response->set_old_urlfile_id(
		mStore.SaveUrlFileAndGetOldId(*response->mutable_urlfile())
	);
	response->set_is_successful(true);

	return Status::OK;
}

Status UrlFileServiceImpl::GetCount(ServerContext * context,
	const ArgVoid * request, ResultWithCount * response)
{
	response->set_count(
		mStore.GetCount()
	);
	response->set_is_successful(true);

	return Status::OK;
}

Status UrlFileServiceImpl::GetForIndex(ServerContext * context,
	const ArgVoid * request, ResultWithUrlFile * response)
{
	response->set_is_successful(
		mStore.GetForIndex(*response->mutable_urlfile())
	);
	
	return Status::OK;
}

Status UrlFileServiceImpl::FinishIndex(ServerContext * context,
	const ArgUrl * request, Result * response)
{
	mStore.FinishIndex(request->url());
	response->set_is_successful(true);

	return Status::OK;
}
