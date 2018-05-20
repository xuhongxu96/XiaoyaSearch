#pragma once

#include "stdafx.h"
#include "BaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		/**
		\brief	Base Class for all classes in Store namespace
		with additional support for Id of models.

		It will append a ColumnFamily in database.
		*/
		class CounterBaseStore
			: public BaseStore
		{
			size_t mCounterCF;	//< Handle index of counter ColumnFamily

			mutable std::shared_mutex mCounterMutex;	// Mutex for update counter;

			/**
			Get value of key without lock (internal use)

			\param	key	Key
			\return	Value
			*/
			uint64_t GetValueInternal(const std::string &key) const;

			/**
			Update value for key without lock (internal use)

			\param	key	Key
			\param	delta	Delta value to update
			*/
			void UpdateValueInternal(const std::string &key, int64_t delta = 1);
		protected:
			/**
			Get value of key

			\param	key	Key
			\return	Value
			*/
			uint64_t GetValue(const std::string &key) const;

			/**
			Update value for key

			\param	key	Key
			\param	delta	Delta value to update
			*/
			void UpdateValue(const std::string &key, int64_t delta = 1);

			/**
			Get value for specific model and update it atomically

			\param	key	Key
			\param	delta	Delta value to update
			\return	Value
			*/
			uint64_t GetAndUpdateValue(const std::string &key, int64_t delta = 1);
		public:
			static const std::string CounterCFName; //< Name of counter ColumnFamily

			/// Get hanlde index of ColumnFamily for Counter
			const size_t GetCounterCF() const;

			/*
			Constructor

			\param	dbFileName	File name of the database bound to the class
			\param	columnFamilyDescriptors	ColumnFamilyDescriptors (Shouldn't include ColumnFamily for Id)
			\param	config		Store configurations
			\param	isReadOnly	Open the database in readonly mode.
			*/
			CounterBaseStore(const std::string &dbFileName,
				const std::vector<rocksdb::ColumnFamilyDescriptor> &columnFamilyDescriptors,
				Config::StoreConfig config,
				bool isReadOnly = false);

			/// Add Id ColumnFamily to existing descriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				AddIdColumnFamilyDescriptor(const std::vector<rocksdb::ColumnFamilyDescriptor> &columnFamilyDescriptors);
		};
	}
}


