#pragma once
#include "stdafx.h"

#include "CounterBaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		class LinkStore : private CounterBaseStore
		{
			/**
			Get Links by url

			\param	url			Url
			\param	outIdList	Output IdList of Links
			\return	Is any link found
			*/
			bool GetLinkIds(const std::string &url, Model::IdList &outIdList) const;

			/**
			Get Links by UrlFile Id

			\param	urlFileId	Id of UrlFile
			\param	outIdList	Output IdList of Links
			\return	Is any link found
			*/
			bool GetLinkIds(const uint64_t urlFileId, Model::IdList &outIdList) const;

			/**
			Clear all links of the specific UrlFile

			\param	urlFileId	Id of UrlFile 
			\param	batch		WriteBatch for atomically writing
			*/
			void ClearLinksOfUrlFile(const uint64_t urlFileId, rocksdb::WriteBatch &batch);

		public:
			static const std::string DbName; //< Database name

			static const std::string MetaMaxLinkId; //< MaxLinkId meta name

			static const std::string UrlIndexCFName; //< Name of url_index ColumnFamily
			static const size_t UrlIndexCF; //< Handle index of url_index ColumnFamily

			static const std::string UrlFileIdIndexCFName; //< Name of urlfile_id_index ColumnFamily
			static const size_t UrlFileIdIndexCF; //< Handle index of urlfile_id_index ColumnFamily

			/**
			Constructor

			\param config
			\param isReadOnly
			*/
			LinkStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Save links of the specific UrlFile

			\param	urlFileId		Id of UrlFile
			\param	oldUrlFileId	Id of old UrlFile with the same url. If no old UrlFile, provides 0.
			\param	links			Links of the UrlFile
			*/
			void SaveLinksOfUrlFile(const uint64_t urlFileId,
				const uint64_t oldUrlFileId, std::vector<Model::Link> links);

			/**
			Get Links by id

			\param	id		Id of Link
			\param	outLink	Output Link
			\return	Is link with the specific id found
			*/
			bool GetLink(const uint64_t id, Model::Link &outLink) const;

			/**
			Get Links by url

			\param	url			Url
			\return	Links with the specific url
			*/
			std::vector<Model::Link> GetLinksByUrl(const std::string &url) const;

			/// Get ColumnFamilyDescriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				GetColumnFamilyDescriptors();
		};

	}
}