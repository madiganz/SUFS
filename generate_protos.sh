gRPCTools/protoc -I proto --csharp_out DataNodeProto proto/DataNodeProto.proto --grpc_out DataNodeProto --plugin=protoc-gen-grpc=gRPCTools/grpc_csharp_plugin
gRPCTools/protoc -I proto --csharp_out ClientProto proto/ClientProto.proto --grpc_out ClientProto --plugin=protoc-gen-grpc=gRPCTools/grpc_csharp_plugin
