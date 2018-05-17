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
				using namespace std::chrono;
				milliseconds ms = duration_cast< milliseconds >(
					system_clock::now().time_since_epoch()
					);
				return ms.count();
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