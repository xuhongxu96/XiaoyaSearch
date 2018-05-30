protoc -I . --cpp_out=. ./models.proto
protoc -I . --cpp_out=. ./rpc.proto

protoc -I . --grpc_out=. ./rpc.proto  --plugin=protoc-gen-grpc=C:\Users\xuhongxu\Source\Libs\vcpkg\installed\x86-windows\tools\grpc\grpc_cpp_plugin.exe

"#include `"stdafx.h`"`n" + (Get-Content models.pb.cc -Raw) | Set-Content models.pb.cc
"#include `"stdafx.h`"`n" + (Get-Content rpc.pb.cc -Raw) | Set-Content rpc.pb.cc
"#include `"stdafx.h`"`n" + (Get-Content rpc.grpc.pb.cc -Raw) | Set-Content rpc.grpc.pb.cc