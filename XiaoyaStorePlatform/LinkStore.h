#pragma once
#include "stdafx.h"

#include "BaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		class LinkStore : private BaseStore
		{
		public:
			static const std::string DbName; //< Database name
			/**
			Constructor

			\param config
			\param isReadOnly
			*/
			LinkStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Save links

			\param	links			Links of the UrlFile
			*/
			void SaveLinks(const std::vector<Model::Link> &links);

			/**
			Remove links

			\param	links			Links of the UrlFile
			*/
			void RemoveLinks(const std::vector<Model::Link> &links);

			/**
			Get Links by url

			\param	url			Url
			\return	Links with the specific url
			*/
			std::vector<Model::Link> GetLinks(const std::string &url) const;

			/// Get ColumnFamilyDescriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				GetColumnFamilyDescriptors();
		};

	}
}