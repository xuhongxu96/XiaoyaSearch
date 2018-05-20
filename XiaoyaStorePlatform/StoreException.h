#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Exception
	{
		class StoreException
		{
			rocksdb::Status mStatus;
			std::string mMessage;
		public:
			StoreException(rocksdb::Status status, std::string &message)
				: mMessage(message), mStatus(status)
			{ }

			StoreException(rocksdb::Status status, const char *message = "") 
				: mMessage(message), mStatus(status)
			{ }

			inline std::string GetMessage()
			{
				return mMessage;
			}

			inline rocksdb::Status GetStatus()
			{
				return mStatus;
			}
		};
	}
}