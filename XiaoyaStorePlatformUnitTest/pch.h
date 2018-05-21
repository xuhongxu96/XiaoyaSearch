//
// pch.h
// Header for standard system include files.
//

#pragma once

#include "gtest/gtest.h"

#include "../XiaoyaStorePlatform/StoreConfig.h"
#include "../XiaoyaStorePlatform/SerializeHelper.h"
#include "../XiaoyaStorePlatform/DateTimeHelper.h"
#include "../XiaoyaStorePlatform/StoreException.h"

#include "../XiaoyaStorePlatform/CounterBaseStore.h"
#include "../XiaoyaStorePlatform/UrlFrontierItemStore.h"
#include "../XiaoyaStorePlatform/UrlFileStore.h"
#include "../XiaoyaStorePlatform/PostingListStore.h"
#include "../XiaoyaStorePlatform/LinkStore.h"

#include <thread>
#include <chrono>

#include <boost\filesystem.hpp>
