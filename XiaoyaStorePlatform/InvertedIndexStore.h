#pragma once
#include "stdafx.h"

#include "BaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		class InvertedIndexStore : private BaseStore
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
			InvertedIndexStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Clear indices of specific UrlFile

			\param	urlFileId	Id of UrlFile
			*/
			void ClearIndices(const uint64_t urlFileId);

			/**
			Save indices of specific UrlFile

			\param	urlFileId	Id of UrlFile
			\param	indices		New Indices to be saved
			*/
			void SaveIndices(const uint64_t urlFileId,
				const std::vector<Model::Index> &indices);

			/**
			Get Index by UrlFile and word

			\param	urlFileId	Id of UrlFile
			\param	word		Word
			*/
			bool GetIndex(const uint64_t urlFileId, const std::string &word, 
				Model::Index &outIndex);

			/// Get ColumnFamilyDescriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				GetColumnFamilyDescriptors();
		};

	}
}