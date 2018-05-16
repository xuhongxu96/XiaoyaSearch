#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Model
	{
		struct ModelCompare
		{
			bool operator() (const UrlFrontierItem &item1, const UrlFrontierItem &item2) const;
		};
	}
}