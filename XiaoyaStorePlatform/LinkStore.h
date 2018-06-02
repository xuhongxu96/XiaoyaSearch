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

			static const std::string UrlFileIdIndexCFName; //< Name of url_file_id_index ColumnFamily
			static const size_t UrlFileIdIndexCF; //< Handle index of url_file_id_index ColumnFamily
			/**
			Constructor

			\param config
			\param isReadOnly
			*/
			LinkStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Save links for specific UrlFile

			\param	urlFileId		Id of UrlFile
			\param	links			Links of the UrlFile
			*/
			void SaveLinks(const uint64_t urlFileId, 
				const std::vector<Model::Link> &links);

			/**
			Clear links of specific UrlFile

			\param	urlFileId	Id of UrlFile
			*/
			void ClearLinks(const uint64_t urlFileId);

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