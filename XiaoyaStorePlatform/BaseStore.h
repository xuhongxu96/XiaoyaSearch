#pragma once

#include "stdafx.h"

#include "StoreConfig.h"
#include "SerializerHelper.h"

namespace XiaoyaStore
{
	namespace Store
	{
		/**
		 \brief	Base Class for all classes in Store namespace
				providing basic DB operations.


		 */
		class BaseStore
		{
			/**
			Open the database bound to the class
			*/
			void OpenDb(const std::vector<rocksdb::ColumnFamilyDescriptor> 
				&columnFamilyDescriptors);
			/// Set mDbPath per StoreConfig and DbFileName
			void SetDbPath(const std::string &dbDirectory,
				const std::string &dbFileName);
		protected:
			Config::StoreConfig mConfig;	//< Store configurations, \sa Config::StoreConfig
			bool mIsReadOnly;	//< Decides whether open the database in readonly mode

			std::string mDbPath;	//< Full path of the database
			std::string mDbFileName;	//< File name of the database (without path)

			std::unique_ptr<rocksdb::DB> mDb;	//< RocksDB Database pointer
			std::vector<std::unique_ptr<rocksdb::ColumnFamilyHandle>> mCFHandles;	//< Stores ColumnFamilyHandles
		public:
			static const std::string DefaultCFName; //< Name of default ColumnFamily
			static const size_t DefaultCF; //< Handle index of default ColumnFamily

			/*
			Constructor

			\param dbFileName File name of the database bound to the class
			\param config Store configurations
			\param isReadOnly Open the database in readonly mode.
			*/
			BaseStore(const std::string &dbFileName,
				const std::vector<rocksdb::ColumnFamilyDescriptor> &columnFamilyDescriptors,
				Config::StoreConfig config,
				bool isReadOnly = false);
		};
	}
}

