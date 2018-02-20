@rem Copyright 2016 gRPC authors.
@rem
@rem Licensed under the Apache License, Version 2.0 (the "License");
@rem you may not use this file except in compliance with the License.
@rem You may obtain a copy of the License at
@rem
@rem     http://www.apache.org/licenses/LICENSE-2.0
@rem
@rem Unless required by applicable law or agreed to in writing, software
@rem distributed under the License is distributed on an "AS IS" BASIS,
@rem WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
@rem See the License for the specific language governing permissions and
@rem limitations under the License.

@rem Generate the C# code for .proto files

setlocal

@rem enter this directory
cd /d %~dp0

gRPCTools\protoc.exe -I proto --csharp_out DataNodeProto  proto\datanodeproto.proto --grpc_out DataNodeProto --plugin=protoc-gen-grpc=gRPCTools\grpc_csharp_plugin.exe

gRPCTools\protoc.exe -I proto --csharp_out ClientProto  proto\clientproto.proto --grpc_out ClientProto --plugin=protoc-gen-grpc=gRPCTools\grpc_csharp_plugin.exe

endlocal
