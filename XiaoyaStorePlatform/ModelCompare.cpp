#include "stdafx.h"
#include "ModelCompare.h"

bool XiaoyaStore::Model::ModelCompare::operator()(const UrlFrontierItem &item1, const UrlFrontierItem &item2) const
{
	auto key1 = std::make_pair(item1.planned_time() / 1000, item1.priority());
	auto key2 = std::make_pair(item2.planned_time() / 1000, item2.priority());
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
