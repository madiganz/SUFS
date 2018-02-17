// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: datanodeproto.proto
// </auto-generated>
#pragma warning disable 1591
#region Designer generated code

using System;
using System.Threading;
using System.Threading.Tasks;
using grpc = global::Grpc.Core;

namespace DataNodeProto {
  public static partial class DataNodeProto
  {
    static readonly string __ServiceName = "DataNodeProto.DataNodeProto";

    static readonly grpc::Marshaller<global::DataNodeProto.HeartBeatRequest> __Marshaller_HeartBeatRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::DataNodeProto.HeartBeatRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::DataNodeProto.HeartBeatResponse> __Marshaller_HeartBeatResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::DataNodeProto.HeartBeatResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::DataNodeProto.BlockReportRequest> __Marshaller_BlockReportRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::DataNodeProto.BlockReportRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::DataNodeProto.StatusResponse> __Marshaller_StatusResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::DataNodeProto.StatusResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::DataNodeProto.DataBlock> __Marshaller_DataBlock = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::DataNodeProto.DataBlock.Parser.ParseFrom);

    static readonly grpc::Method<global::DataNodeProto.HeartBeatRequest, global::DataNodeProto.HeartBeatResponse> __Method_SendHeartBeat = new grpc::Method<global::DataNodeProto.HeartBeatRequest, global::DataNodeProto.HeartBeatResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SendHeartBeat",
        __Marshaller_HeartBeatRequest,
        __Marshaller_HeartBeatResponse);

    static readonly grpc::Method<global::DataNodeProto.BlockReportRequest, global::DataNodeProto.StatusResponse> __Method_SendBlockReport = new grpc::Method<global::DataNodeProto.BlockReportRequest, global::DataNodeProto.StatusResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SendBlockReport",
        __Marshaller_BlockReportRequest,
        __Marshaller_StatusResponse);

    static readonly grpc::Method<global::DataNodeProto.DataBlock, global::DataNodeProto.StatusResponse> __Method_WriteDataBlock = new grpc::Method<global::DataNodeProto.DataBlock, global::DataNodeProto.StatusResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "WriteDataBlock",
        __Marshaller_DataBlock,
        __Marshaller_StatusResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::DataNodeProto.DatanodeprotoReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of DataNodeProto</summary>
    public abstract partial class DataNodeProtoBase
    {
      public virtual global::System.Threading.Tasks.Task<global::DataNodeProto.HeartBeatResponse> SendHeartBeat(global::DataNodeProto.HeartBeatRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::DataNodeProto.StatusResponse> SendBlockReport(global::DataNodeProto.BlockReportRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::DataNodeProto.StatusResponse> WriteDataBlock(global::DataNodeProto.DataBlock request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for DataNodeProto</summary>
    public partial class DataNodeProtoClient : grpc::ClientBase<DataNodeProtoClient>
    {
      /// <summary>Creates a new client for DataNodeProto</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public DataNodeProtoClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for DataNodeProto that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public DataNodeProtoClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected DataNodeProtoClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected DataNodeProtoClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::DataNodeProto.HeartBeatResponse SendHeartBeat(global::DataNodeProto.HeartBeatRequest request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return SendHeartBeat(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::DataNodeProto.HeartBeatResponse SendHeartBeat(global::DataNodeProto.HeartBeatRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SendHeartBeat, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::DataNodeProto.HeartBeatResponse> SendHeartBeatAsync(global::DataNodeProto.HeartBeatRequest request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return SendHeartBeatAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::DataNodeProto.HeartBeatResponse> SendHeartBeatAsync(global::DataNodeProto.HeartBeatRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SendHeartBeat, null, options, request);
      }
      public virtual global::DataNodeProto.StatusResponse SendBlockReport(global::DataNodeProto.BlockReportRequest request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return SendBlockReport(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::DataNodeProto.StatusResponse SendBlockReport(global::DataNodeProto.BlockReportRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SendBlockReport, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::DataNodeProto.StatusResponse> SendBlockReportAsync(global::DataNodeProto.BlockReportRequest request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return SendBlockReportAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::DataNodeProto.StatusResponse> SendBlockReportAsync(global::DataNodeProto.BlockReportRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SendBlockReport, null, options, request);
      }
      public virtual global::DataNodeProto.StatusResponse WriteDataBlock(global::DataNodeProto.DataBlock request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return WriteDataBlock(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::DataNodeProto.StatusResponse WriteDataBlock(global::DataNodeProto.DataBlock request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_WriteDataBlock, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::DataNodeProto.StatusResponse> WriteDataBlockAsync(global::DataNodeProto.DataBlock request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return WriteDataBlockAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::DataNodeProto.StatusResponse> WriteDataBlockAsync(global::DataNodeProto.DataBlock request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_WriteDataBlock, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override DataNodeProtoClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new DataNodeProtoClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(DataNodeProtoBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_SendHeartBeat, serviceImpl.SendHeartBeat)
          .AddMethod(__Method_SendBlockReport, serviceImpl.SendBlockReport)
          .AddMethod(__Method_WriteDataBlock, serviceImpl.WriteDataBlock).Build();
    }

  }
}
#endregion
