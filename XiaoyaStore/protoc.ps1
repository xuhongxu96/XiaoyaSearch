protoc -I ../XiaoyaStorePlatform --csharp_out=. ../XiaoyaStorePlatform/models.proto
protoc -I ../XiaoyaStorePlatform --csharp_out=. ../XiaoyaStorePlatform/rpc.proto

protoc -I ../XiaoyaStorePlatform --grpc_out=. ../XiaoyaStorePlatform/rpc.proto  --plugin=protoc-gen-grpc=C:\Users\xuhongxu\Source\Libs\vcpkg\installed\x86-windows\tools\grpc\grpc_csharp_plugin.exe