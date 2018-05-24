#pragma once
#include "stdafx.h"

#include "CounterBaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		class InvertedIndexStore : private CounterBaseStore
		{
			/**
			Get Index by id

			\param	id			Id of Index
			\param	outIndex	Output Index
			\return	Is index successfully found
			*/
			bool GetIndex(const uint64_t id, Model::Index &outIndex) const;

			/**
			Get IdList of Indices of specific UrlFile

			\param	urlFileId	Id of UrlFile
			\param	outIdList	Output IdList of Indices
			\return	Is indices successfully found
			*/
			bool GetIndexIds(const uint64_t urlFileId, Model::IdList &outIdList) const;

			/**
			Get Index by IndexKey

			\param	indexKey	IndexKey	
			\param	outIndex	Output Index
			\return	Is index successfully found
			*/
			bool GetIndex(const Model::IndexKey &indexKey, Model::Index &outIndex) const;

			/**
			Clear Indices of specific UrlFile

			\param	urlFileId	Id of UrlFile
			*/
			void ClearIndicesOf(const uint64_t urlFileId, rocksdb::WriteBatch &batch);
		public:
			static const std::string DbName; //< Database name

			static const std::string MetaMaxIndexId; //< MaxLinkId meta name

			static const std::string IndexKeyCFName; //< Name of index_key ColumnFamily
			static const size_t IndexKeyCF; //< Handle index of index_key ColumnFamily

			static const std::string UrlFileIdIndexCFName; //< Name of urlfile_id_index ColumnFamily
			static const size_t UrlFileIdIndexCF; //< Handle index of urlfile_id_index ColumnFamily

			/**
			Constructor

			\param config
			\param isReadOnly
			*/
			InvertedIndexStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Clear and save new Indices of specific UrlFile

			\param	urlFileId		Id of UrlFile
			\param	oldUrlFileId	Id of old UrlFile with the same url
			\param	indices			New Indices to be saved
			*/
			void ClearAndSaveIndicesOf(const uint64_t urlFileId,
				const uint64_t oldUrlFileId, const std::vector<Model::Index> &indices);

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