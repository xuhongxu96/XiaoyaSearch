#pragma once
#include "stdafx.h"

#include "BaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		class PostingListStore : private BaseStore
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
			PostingListStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Save posting lists by specific delta posting lists,
			PostingList.IsAdd indicates whether you want to add or 
			remove the deltaPostingList from the original 
			posting list of the same word.
			WordFrequency and DocumentFrequency will be also added or 
			removed per PostingList.IsAdd.

			\param	urlFileId			Id of UrlFile
			\param	deltaPostingLists	A list of delta change of PostingList
			*/
			void SavePostingLists(const uint64_t urlFileId, 
				const std::vector<Model::PostingList> &deltaPostingLists);

			/**
			Clear posting lists of specific UrlFile

			\param	urlFileId	Id of UrlFile
			*/
			void ClearPostingLists(const uint64_t urlFileId);

			/**
			Get PostingList by word

			\param	word	Word
			\param	outPostingList	Output PostingList

			\return	Return true if posting list is successfully loaded
			*/
			bool GetPostingList(const std::string &word, Model::PostingList &outPostingList) const;

			/// Get ColumnFamilyDescriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				GetColumnFamilyDescriptors();
		};

	}
}