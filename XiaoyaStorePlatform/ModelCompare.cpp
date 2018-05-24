#include "stdafx.h"
#include "ModelCompare.h"

bool XiaoyaStore::Model::ModelCompare::operator()(const UrlFrontierItem &item1, const UrlFrontierItem &item2) const
{
	auto key1 = std::make_pair(item1.planned_time() / 1000, item1.priority());
	auto key2 = std::make_pair(item2.planned_time() / 1000, item2.priority());
	auto result = key1 > key2;
	return result;
}

bool XiaoyaStore::Model::ModelCompare::operator()(const UrlFile & item1, const UrlFile & item2) const
{
	return item1.updated_at() > item2.updated_at();
}
