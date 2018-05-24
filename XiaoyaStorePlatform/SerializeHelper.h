#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Helper
	{
		class SerializeHelper
		{
		public:
			template <typename T>
			static typename boost::enable_if<
				std::is_base_of<::google::protobuf::Message, T>,
				T>::type
			Deserialize(const std::string &data)
			{
				T model;
				model.ParseFromString(data);

				return std::move(model);
			}

			template <typename T>
			static typename boost::enable_if<
				std::is_base_of<::google::protobuf::Message, T>,
				std::string>::type
				Serialize(const T& model)
			{
				std::string data;
				model.SerializeToString(&data);
				return std::move(data);
			}

			static uint64_t DeserializeUInt64(const std::string &data)
			{
				return std::bitset<sizeof(uint64_t) * 8>(data).to_ullong();
			}

			static int64_t DeserializeInt64(const std::string &data)
			{
				return static_cast<int64_t>(std::bitset<sizeof(uint64_t) * 8>(data).to_ullong());
			}

			static std::string SerializeInt64(const int64_t num)
			{
				return std::move(std::bitset<sizeof(int64_t) * 8>(num).to_string());
			}

			static std::string SerializeUInt64(const uint64_t num)
			{
				return std::move(std::bitset<sizeof(uint64_t) * 8>(num).to_string());
			}
		};
	}
}