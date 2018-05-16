#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Exception
	{
		class StoreException
		{
			std::string mMessage;
		public:
			StoreException(std::string &message) : mMessage(message)
			{ }

			StoreException(const char *message = "") : mMessage(message)
			{ }

			inline std::string GetMessage()
			{
				return mMessage;
			}
		};
	}
}