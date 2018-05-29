#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Config
	{
		struct StoreConfig
		{
			bool EnableExactPlannedTime = true;
			std::string StoreDirectory;
		};
	}
}