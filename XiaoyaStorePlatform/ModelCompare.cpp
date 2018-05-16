#include "stdafx.h"
#include "ModelCompare.h"

bool XiaoyaStore::Model::ModelCompare::operator()(const UrlFrontierItem &item1, const UrlFrontierItem &item2) const
{
	auto key1 = std::make_pair(item1.PlannedTime, item1.Priority);
	auto key2 = std::make_pair(item2.PlannedTime, item2.Priority);

	return key1 < key2;
}
