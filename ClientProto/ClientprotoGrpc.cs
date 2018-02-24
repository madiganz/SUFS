// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: clientproto.proto
#pragma warning disable 1591
#region Designer generated code

using System;
using System.Threading;
using System.Threading.Tasks;
using grpc = global::Grpc.Core;

namespace ClientProto {
  public static partial class ClientProto
  {
    static readonly string __ServiceName = "ClientProto.ClientProto";

    static readonly grpc::Marshaller<global::ClientProto.Path> __Marshaller_Path = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.Path.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.StatusResponse> __Marshaller_StatusResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.StatusResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.ListOfNodes> __Marshaller_ListOfNodes = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.ListOfNodes.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.NewFile> __Marshaller_NewFile = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.NewFile.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.BlockMessage> __Marshaller_BlockMessage = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.BlockMessage.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.ListOfContents> __Marshaller_ListOfContents = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.ListOfContents.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.UUID> __Marshaller_UUID = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.UUID.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.BlockData> __Marshaller_BlockData = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.BlockData.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::ClientProto.BlockInfo> __Marshaller_BlockInfo = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::ClientProto.BlockInfo.Parser.ParseFrom);

    static readonly grpc::Method<global::ClientProto.Path, global::ClientProto.StatusResponse> __Method_DeleteDirectory = new grpc::Method<global::ClientProto.Path, global::ClientProto.StatusResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "DeleteDirectory",
        __Marshaller_Path,
        __Marshaller_StatusResponse);

    static readonly grpc::Method<global::ClientProto.Path, global::ClientProto.StatusResponse> __Method_AddDirectory = new grpc::Method<global::ClientProto.Path, global::ClientProto.StatusResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "AddDirectory",
        __Marshaller_Path,
        __Marshaller_StatusResponse);

    static readonly grpc::Method<global::ClientProto.Path, global::ClientProto.ListOfNodes> __Method_ListNodes = new grpc::Method<global::ClientProto.Path, global::ClientProto.ListOfNodes>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ListNodes",
        __Marshaller_Path,
        __Marshaller_ListOfNodes);

    static readonly grpc::Method<global::ClientProto.NewFile, global::ClientProto.BlockMessage> __Method_CreateFile = new grpc::Method<global::ClientProto.NewFile, global::ClientProto.BlockMessage>(
        grpc::MethodType.Unary,
        __ServiceName,
        "CreateFile",
        __Marshaller_NewFile,
        __Marshaller_BlockMessage);

    static readonly grpc::Method<global::ClientProto.Path, global::ClientProto.StatusResponse> __Method_DeleteFile = new grpc::Method<global::ClientProto.Path, global::ClientProto.StatusResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "DeleteFile",
        __Marshaller_Path,
        __Marshaller_StatusResponse);

    static readonly grpc::Method<global::ClientProto.Path, global::ClientProto.ListOfContents> __Method_ListContents = new grpc::Method<global::ClientProto.Path, global::ClientProto.ListOfContents>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ListContents",
        __Marshaller_Path,
        __Marshaller_ListOfContents);

    static readonly grpc::Method<global::ClientProto.UUID, global::ClientProto.BlockData> __Method_ReadBlock = new grpc::Method<global::ClientProto.UUID, global::ClientProto.BlockData>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "ReadBlock",
        __Marshaller_UUID,
        __Marshaller_BlockData);

    static readonly grpc::Method<global::ClientProto.BlockData, global::ClientProto.StatusResponse> __Method_WriteBlock = new grpc::Method<global::ClientProto.BlockData, global::ClientProto.StatusResponse>(
        grpc::MethodType.ClientStreaming,
        __ServiceName,
        "WriteBlock",
        __Marshaller_BlockData,
        __Marshaller_StatusResponse);

    static readonly grpc::Method<global::ClientProto.BlockInfo, global::ClientProto.StatusResponse> __Method_GetReady = new grpc::Method<global::ClientProto.BlockInfo, global::ClientProto.StatusResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetReady",
        __Marshaller_BlockInfo,
        __Marshaller_StatusResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::ClientProto.ClientprotoReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of ClientProto</summary>
    public abstract partial class ClientProtoBase
    {
      /// <summary>
      ///Delete a directory 
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ClientProto.StatusResponse> DeleteDirectory(global::ClientProto.Path request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      ///Create a directory
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ClientProto.StatusResponse> AddDirectory(global::ClientProto.Path request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// List the DataNodes that store replicas of each block of a file
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ClientProto.ListOfNodes> ListNodes(global::ClientProto.Path request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      ///Create a new file in SUFS
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ClientProto.BlockMessage> CreateFile(global::ClientProto.NewFile request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      ///Delete a file
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ClientProto.StatusResponse> DeleteFile(global::ClientProto.Path request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      ///List the contents of a directory
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ClientProto.ListOfContents> ListContents(global::ClientProto.Path request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// Reads block from datanode
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="responseStream">Used for sending responses back to the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>A task indicating completion of the handler.</returns>
      public virtual global::System.Threading.Tasks.Task ReadBlock(global::ClientProto.UUID request, grpc::IServerStreamWriter<global::ClientProto.BlockData> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// Creates a pipeline to stream bytes to datanodes
      /// </summary>
      /// <param name="requestStream">Used for reading requests from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ClientProto.StatusResponse> WriteBlock(grpc::IAsyncStreamReader<global::ClientProto.BlockData> requestStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::ClientProto.StatusResponse> GetReady(global::ClientProto.BlockInfo request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }
    }

    /// <summary>Client for ClientProto</summary>
    public partial class ClientProtoClient : grpc::ClientBase<ClientProtoClient>
    {
      /// <summary>Creates a new client for ClientProto</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public ClientProtoClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for ClientProto that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public ClientProtoClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected ClientProtoClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected ClientProtoClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      /// <summary>
      ///Delete a directory 
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.StatusResponse DeleteDirectory(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return DeleteDirectory(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Delete a directory 
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.StatusResponse DeleteDirectory(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_DeleteDirectory, null, options, request);
      }
      /// <summary>
      ///Delete a directory 
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> DeleteDirectoryAsync(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return DeleteDirectoryAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Delete a directory 
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> DeleteDirectoryAsync(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_DeleteDirectory, null, options, request);
      }
      /// <summary>
      ///Create a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.StatusResponse AddDirectory(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return AddDirectory(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Create a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.StatusResponse AddDirectory(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_AddDirectory, null, options, request);
      }
      /// <summary>
      ///Create a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> AddDirectoryAsync(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return AddDirectoryAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Create a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> AddDirectoryAsync(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_AddDirectory, null, options, request);
      }
      /// <summary>
      /// List the DataNodes that store replicas of each block of a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.ListOfNodes ListNodes(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return ListNodes(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// List the DataNodes that store replicas of each block of a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.ListOfNodes ListNodes(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ListNodes, null, options, request);
      }
      /// <summary>
      /// List the DataNodes that store replicas of each block of a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.ListOfNodes> ListNodesAsync(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return ListNodesAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// List the DataNodes that store replicas of each block of a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.ListOfNodes> ListNodesAsync(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ListNodes, null, options, request);
      }
      /// <summary>
      ///Create a new file in SUFS
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.BlockMessage CreateFile(global::ClientProto.NewFile request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return CreateFile(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Create a new file in SUFS
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.BlockMessage CreateFile(global::ClientProto.NewFile request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_CreateFile, null, options, request);
      }
      /// <summary>
      ///Create a new file in SUFS
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.BlockMessage> CreateFileAsync(global::ClientProto.NewFile request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return CreateFileAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Create a new file in SUFS
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.BlockMessage> CreateFileAsync(global::ClientProto.NewFile request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_CreateFile, null, options, request);
      }
      /// <summary>
      ///Delete a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.StatusResponse DeleteFile(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return DeleteFile(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Delete a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.StatusResponse DeleteFile(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_DeleteFile, null, options, request);
      }
      /// <summary>
      ///Delete a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> DeleteFileAsync(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return DeleteFileAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///Delete a file
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> DeleteFileAsync(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_DeleteFile, null, options, request);
      }
      /// <summary>
      ///List the contents of a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.ListOfContents ListContents(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return ListContents(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///List the contents of a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::ClientProto.ListOfContents ListContents(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ListContents, null, options, request);
      }
      /// <summary>
      ///List the contents of a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.ListOfContents> ListContentsAsync(global::ClientProto.Path request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return ListContentsAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///List the contents of a directory
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::ClientProto.ListOfContents> ListContentsAsync(global::ClientProto.Path request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ListContents, null, options, request);
      }
      /// <summary>
      /// Reads block from datanode
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncServerStreamingCall<global::ClientProto.BlockData> ReadBlock(global::ClientProto.UUID request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return ReadBlock(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Reads block from datanode
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncServerStreamingCall<global::ClientProto.BlockData> ReadBlock(global::ClientProto.UUID request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_ReadBlock, null, options, request);
      }
      /// <summary>
      /// Creates a pipeline to stream bytes to datanodes
      /// </summary>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncClientStreamingCall<global::ClientProto.BlockData, global::ClientProto.StatusResponse> WriteBlock(grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return WriteBlock(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Creates a pipeline to stream bytes to datanodes
      /// </summary>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncClientStreamingCall<global::ClientProto.BlockData, global::ClientProto.StatusResponse> WriteBlock(grpc::CallOptions options)
      {
        return CallInvoker.AsyncClientStreamingCall(__Method_WriteBlock, null, options);
      }
      public virtual global::ClientProto.StatusResponse GetReady(global::ClientProto.BlockInfo request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return GetReady(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::ClientProto.StatusResponse GetReady(global::ClientProto.BlockInfo request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetReady, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> GetReadyAsync(global::ClientProto.BlockInfo request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return GetReadyAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::ClientProto.StatusResponse> GetReadyAsync(global::ClientProto.BlockInfo request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetReady, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override ClientProtoClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new ClientProtoClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(ClientProtoBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_DeleteDirectory, serviceImpl.DeleteDirectory)
          .AddMethod(__Method_AddDirectory, serviceImpl.AddDirectory)
          .AddMethod(__Method_ListNodes, serviceImpl.ListNodes)
          .AddMethod(__Method_CreateFile, serviceImpl.CreateFile)
          .AddMethod(__Method_DeleteFile, serviceImpl.DeleteFile)
          .AddMethod(__Method_ListContents, serviceImpl.ListContents)
          .AddMethod(__Method_ReadBlock, serviceImpl.ReadBlock)
          .AddMethod(__Method_WriteBlock, serviceImpl.WriteBlock)
          .AddMethod(__Method_GetReady, serviceImpl.GetReady).Build();
    }

  }
}
#endregion
