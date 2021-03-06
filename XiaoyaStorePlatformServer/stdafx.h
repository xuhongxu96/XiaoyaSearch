// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#include <cstdio>
#include <tchar.h>

#include "../XiaoyaStorePlatform/StoreConfig.h"
#include "../XiaoyaStorePlatform/SerializeHelper.h"
#include "../XiaoyaStorePlatform/DateTimeHelper.h"
#include "../XiaoyaStorePlatform/StoreException.h"

#include "../XiaoyaStorePlatform/CounterBaseStore.h"
#include "../XiaoyaStorePlatform/UrlFrontierItemStore.h"
#include "../XiaoyaStorePlatform/UrlFileStore.h"
#include "../XiaoyaStorePlatform/PostingListStore.h"
#include "../XiaoyaStorePlatform/LinkStore.h"
#include "../XiaoyaStorePlatform/InvertedIndexStore.h"

#include "../XiaoyaStorePlatform/UrlFrontierItemServiceImpl.h"
#include "../XiaoyaStorePlatform/UrlFileServiceImpl.h"
#include "../XiaoyaStorePlatform/PostingListServiceImpl.h"
#include "../XiaoyaStorePlatform/LinkServiceImpl.h"
#include "../XiaoyaStorePlatform/InvertedIndexServiceImpl.h"

#include <thread>
#include <chrono>
#include <future>
#include <algorithm>
#include <string>

#include <boost\filesystem.hpp>
