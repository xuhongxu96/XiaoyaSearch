#pragma once

#include "stdafx.h"

#include "BaseStore.h"
#include "StoreConfig.h"

namespace XiaoyaStore
{
	namespace Store
	{
		class UrlFrontierItemStore
			: private BaseStore
		{
			mutable std::shared_mutex mSharedMutexForQueue;
			mutable std::shared_mutex mSharedMutexForPop;

			/// All items in url frontier, 
			/// sorted by PlannedTime and Priority.
			std::priority_queue <Model::UrlFrontierItem, std::vector<Model::UrlFrontierItem>,
				Model::ModelCompare> mUrlQueue;

			/// A set containing all urls in url frontier.
			std::unordered_set<std::string> mUrlSet;

			/// A map to save all popped url frontier items.
			std::unordered_map<std::string, 
				Model::UrlFrontierItem> mPoppedUrlMap;

			Model::UrlFrontierItem CreateItem(const std::string &url) const;

			/**
			Save new UrlFrontierItem into database atomically
			\param	item	UrlFrontierItem to be saved
			*/
			void SaveNewItem(Model::UrlFrontierItem &item);

			/**
			Update existed UrlFrontierItem into database
			\param	item	UrlFrontierItem to be updated
			*/
			void UpdateItem(Model::UrlFrontierItem &item);

			/**
			Add UrlFrontierItem to mUrlQueue and mUrlSet atomically
			
			\param	item	UrlFrontierItem to be added
			*/
			void AddToQueue(Model::UrlFrontierItem &item);

			/// Is url in mUrlSet
			bool HasUrl(const std::string &url) const;
			/// Is url popped (i.e. in mPoppedUrlMap)
			bool IsPopped(const std::string &url) const;
			/**
			Try remove the specific url in mPoppedUrlMap, 
			and give the corresponding UrlFrontierItem.

			\param	url	Url
			\param	outItem	Output UrlFrontierItem
			\return	Is url successfully removed
			*/
			bool TryRemovePoppedUrl(const std::string &url, Model::UrlFrontierItem &outItem);
			/**
			Remove url from mUrlSet and mPoppedUrlMap

			\param	url	Url
			*/
			void RemoveUrlInternal(const std::string &url);
		public:
			static const std::string DbName; //< Database name

			static const std::string HostCountCFName; //< Name of host_count ColumnFamily
			static const size_t HostCountCF; //< Handle index of host_count ColumnFamily
			/**
			Constructor

			\param config
			\param isReadOnly
			*/
			UrlFrontierItemStore(Config::StoreConfig config,
				bool isReadOnly = false);

			/**
			Init by adding UrlFrontierItems of specific initial urls.
			All PlannedTime will be set to now. Already Existed urls will be ignored.
			\sa PushUrls requires adding locks and will re-calculate PlannedTime.

			\param	urls	Initial urls
			*/
			void Init(const std::vector<std::string> &urls);
			/// Load UrlFrontierItems from database
			void ReloadUrlFrontierItems();
			/**
			Push new urls into UrlFrontier. Already existed urls will be ignored.

			\param	urls	Urls to be pushed
			*/
			void PushUrls(const std::vector<std::string> &urls);
			/**
			Push popped url back and re-calculate its PlannedTime.
			If the url is already pushed back or is not popped, 
			this method will do nothing and return false.

			\param	url		Url to be pushed back
			\param	updateInterval	The update interval of web content of the url
			\param	failed	Is failed to fetch the web content of the url

			\return	Is the url successfully pushed back
			*/
			bool PushBackUrl(const std::string url, uint64_t updateInterval, bool failed = false);
			/**
			Pop a url, generally for crawling.

			\param	url	Output url
			\return	Is url successfully popped
			*/
			const bool PopUrl(std::string &outUrl);

			/**
			Remove a url from database

			\param	url	Url
			*/
			void RemoveUrl(const std::string &url);

			/**
			Get the number of the urls with specific host name.

			\param	host	Host name
			
			\return The number of the urls with specific host name
			*/
			uint64_t GetHostCount(const std::string &host);

			/// Get ColumnFamilyDescriptors
			static const std::vector<rocksdb::ColumnFamilyDescriptor>
				GetColumnFamilyDescriptors();
		};
	}
}