#include "stdafx.h"
#include "ModelCompare.h"

bool XiaoyaStore::Model::ModelCompare::operator()(const UrlFrontierItem &item1, const UrlFrontierItem &item2) const
{
	auto key1 = std::make_pair(item1.PlannedTime / 1000, item1.Priority);
	auto key2 = std::make_pair(item2.PlannedTime / 1000, item2.Priority);
	auto result = key1 > key2;
	return result;
}
