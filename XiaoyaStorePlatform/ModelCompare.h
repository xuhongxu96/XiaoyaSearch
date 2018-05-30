#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Model
	{
		struct ModelCompare
		{
			bool operator() (const UrlFrontierItem &item1, const UrlFrontierItem &item2) const;

			bool operator() (const UrlFile &item1, const UrlFile &item2) const;

			bool operator() (const Link &item1, const Link &item2) const;
		};
	}
}