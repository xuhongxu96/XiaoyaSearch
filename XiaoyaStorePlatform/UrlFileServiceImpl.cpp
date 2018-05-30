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
		mStore.GetUrlFile(request->id(), *response->mutable_url_file())
	);
	return Status::OK;
}

Status UrlFileServiceImpl::GetUrlFileByUrl(ServerContext * context,
	const ArgUrl * request, ResultWithUrlFile * response)
{
	response->set_is_successful(
		mStore.GetUrlFile(request->url(), *response->mutable_url_file())
	);
	return Status::OK;
}

Status UrlFileServiceImpl::GetUrlFilesByHash(ServerContext * context,
	const ArgHash * request, ResultWithUrlFiles * response)
{
	auto urlFiles = mStore.GetUrlFilesByHash(request->hash());

	*response->mutable_url_files() 
		= ::google::protobuf::RepeatedPtrField<UrlFile>(urlFiles.begin(), urlFiles.end());
	response->set_is_successful(true);

	return Status::OK;
}

Status UrlFileServiceImpl::SaveUrlFileAndGetOldId(ServerContext * context,
	const ArgUrlFile * request, ResultWithUrlFileAndOldId * response)
{
	response->mutable_url_file()->CopyFrom(request->url_file());
	response->set_old_url_file_id(
		mStore.SaveUrlFileAndGetOldId(*response->mutable_url_file())
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

Status UrlFileServiceImpl::ContainsId(ServerContext * context,
	const ArgId * request, Result * response)
{
	response->set_is_successful(
		mStore.ContainsId(request->id())
	);
	return Status::OK;
}