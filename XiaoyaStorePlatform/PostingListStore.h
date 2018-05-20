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

			/**
			Constructor

			\param config
			\param isReadOnly
			*/
			PostingListStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Save posting list by specific delta posting list,
			PostingList.IsAdd indicates whether you want to add or 
			remove the deltaPostingList from the original 
			posting list of the same word.
			WordFrequency and DocumentFrequency will always be added up 
			regardless of PostingList.IsAdd, so you may set them to negative numbers.

			\param	deltaPostingList	Delta change of PostingList
			*/
			void SavePostingList(Model::PostingList &deltaPostingList);

			/**
			Load PostingList by word

			\param	word	Word
			\param	outPostingList	Output PostingList

			\return	Return true if posting list is successfully loaded
			*/
			bool LoadPostingList(std::string &word, Model::PostingList &outPostingList);

			/// Get ColumnFamilyDescriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				GetColumnFamilyDescriptors();
		};

	}
}