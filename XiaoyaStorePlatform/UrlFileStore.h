#pragma once

#include "stdafx.h"

#include "CounterBaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		class UrlFileStore
			: private CounterBaseStore
		{
			/**
			Get max id of UrlFile

			\return	Max id
			*/
			uint64_t GetMaxUrlFileId() const;

			/**
			Get IdList of UrlFiles by hash

			\param	hash		Hash code of UrlFiles
			\param	outIdList	Output id list of UrlFiles with specific hash code
			\return Is the hash found
			*/
			bool GetUrlFileIdListByHash(const std::string &hash, Model::IdList &outIdList) const;
		public:
			static const std::string DbName; //< Database name

			static const std::string MetaMaxUrlFileId; //< MaxUrlFileId meta name

			static const std::string UrlIndexCFName; //< Name of url_index ColumnFamily
			static const size_t UrlIndexCF; //< Handle index of url_index ColumnFamily

			static const std::string HashIndexCFName; //< Name of hash_index ColumnFamily
			static const size_t HashIndexCF; //< Handle index of hash_index ColumnFamily

			/**
			Constructor

			\param config
			\param isReadOnly
			*/
			UrlFileStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Get UrlFile by id
			
			\param	id	Id of UrlFile
			\param	outUrlFile	Output UrlFile
			\return	Is specific UrlFile found
			*/
			bool GetUrlFile(const uint64_t urlFileId, Model::UrlFile &outUrlFile) const;

			/**
			Get UrlFile by url

			\param	id	Url of UrlFile
			\param	outUrlFile	Output UrlFile
			\return	Is specific UrlFile found
			*/
			bool GetUrlFile(const std::string &url, Model::UrlFile &outUrlFile) const;

			/**
			Get UrlFiles by hash
			
			\param	hash	Hash code of UrlFiles
			\return	UrlFiles with specific hash code
			*/
			std::vector<Model::UrlFile> GetUrlFilesByHash(const std::string &hash);

			/**
			Save new UrlFile and get id of old UrlFile with the same url

			\param	inOutUrlFile	Pass in new UrlFile and output it with its assigned Id.
			\return	Id of old UrlFile which has the same url with the new UrlFile. If no old UrlFile, return 0.
			*/
			uint64_t SaveUrlFileAndGetOldId(Model::UrlFile &inOutUrlFile);

			/**
			Get the number of UrlFiles 
			(Use \sa GetMaxUrlFileId to get approximate count)

			\return	The number of UrlFiles
			*/
			uint64_t GetCount() const;

			/**
			Is specific Id valid

			\param	id	Id of UrlFile
			\return	Is specific UrlFile a valid UrlFile (i.e. not replaced or removed)
			*/
			bool ContainsId(const uint64_t id);

			/// Get ColumnFamilyDescriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				GetColumnFamilyDescriptors();
		};
	}
}
