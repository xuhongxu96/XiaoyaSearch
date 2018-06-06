#include "stdafx.h"
#include "ModelCompare.h"

bool XiaoyaStore::Model::ModelCompare::operator()(const UrlFrontierItem &item1, const UrlFrontierItem &item2) const
{
	auto key1 = std::make_tuple(item1.planned_time() / 2000, item1.priority(), item1.planned_time());
	auto key2 = std::make_tuple(item2.planned_time() / 2000, item2.priority(), item2.planned_time());
	return key1 > key2;
}

bool XiaoyaStore::Model::ModelCompare::operator()(const UrlFile & item1, const UrlFile & item2) const
{
	return item1.updated_at() > item2.updated_at();
}

bool XiaoyaStore::Model::ModelCompare::operator()(const Link & item1, const Link & item2) const
{
	auto key1 = std::make_tuple(item1.url_file_id(), item1.url(), item1.text());
	auto key2 = std::make_tuple(item2.url_file_id(), item2.url(), item2.text());
	return key1 < key2;
}

bool XiaoyaStore::Model::ModelCompare::operator()(const Posting & item1, const Posting & item2) const
{
	auto key1 = std::make_tuple(item1.weight(), item1.url_file_id());
	auto key2 = std::make_tuple(item2.weight(), item2.url_file_id());
	return key1 < key2;
}
