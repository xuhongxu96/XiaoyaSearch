// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#pragma comment(lib, "Ws2_32.lib")
#pragma comment(lib, "RpcRT4.Lib")
#pragma comment(lib, "Shlwapi.lib")

#include "targetver.h"

#include <cstdio>
#include <cstdint>
#include <ctime>

#include <concurrent_priority_queue.h>
#include <concurrent_unordered_set.h>
#include <concurrent_unordered_map.h>
#include <concurrent_queue.h>

#include <array>
#include <vector>
#include <string>
#include <bitset>
#include <queue>
#include <unordered_set>
#include <unordered_map>
#include <map>

#include <memory>
#include <mutex>
#include <algorithm>
#include <functional>
#include <utility>
#include <mutex>
#include <shared_mutex>
#include <type_traits>

#include <rocksdb\db.h>
#include <rocksdb\options.h>
#include <rocksdb\slice.h>
#include <rocksdb\merge_operator.h>
#include <rocksdb\slice.h>
#include <rocksdb\write_batch.h>

#include <boost\filesystem.hpp>

#include <uriparser\Uri.h>

#include <grpcpp/grpcpp.h>
#include "models.pb.h"
#include "rpc.pb.h"
#include "rpc.grpc.pb.h"

#include "ModelCompare.h"