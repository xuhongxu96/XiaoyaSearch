#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Helper
	{
		class DateTimeHelper
		{
		public:
			static inline uint64_t Now()
			{
				return static_cast<uint16_t>(std::time(nullptr));
			}

			static inline int64_t FromSeconds(double seconds)
			{
				return static_cast<int64_t>(seconds * 1000);
			}

			static inline int64_t FromMinutes(double minutes)
			{
				return FromSeconds(minutes * 60);
			}

			static inline int64_t FromHours(double hours)
			{
				return FromMinutes(hours * 60);
			}

			static inline int64_t FromDays(double days)
			{
				return FromHours(days * 24);
			}
		};
	}
}