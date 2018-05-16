#pragma once

#include "stdafx.h"

namespace XiaoyaStore
{
	namespace Helper
	{
		class SerializerHelper
		{
		public:
			template <typename T>
			static T Deserialize(const std::string &data)
			{
				auto charArr = data.c_str();

				bond::InputBuffer input(charArr, data.length());
				bond::FastBinaryReader<bond::InputBuffer> reader(input);

				T model;
				bond::Deserialize(reader, model);
				return model;
			}

			template <typename T>
			static typename boost::enable_if<std::is_class<typename T::Schema::fields>, std::string>::type
				Serialize(const T& model)
			{
				bond::OutputBuffer output;
				bond::FastBinaryWriter<bond::OutputBuffer> writer(output);

				bond::Serialize(model, writer);

				auto blob = output.GetBuffer();
				return std::string(blob.begin(), blob.end());
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
				return std::bitset<sizeof(int64_t) * 8>(num).to_string();
			}

			static std::string SerializeUInt64(const uint64_t num)
			{
				return std::bitset<sizeof(uint64_t) * 8>(num).to_string();
			}
		};
	}
}