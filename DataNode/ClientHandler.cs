using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    class ClientHandler : ClientProto.ClientProto.ClientProtoBase
    {
        public override Task<ClientProto.StatusResponse> GetReady(ClientProto.BlockInfo blockInfo, ServerCallContext context)
        {
            ClientProto.StatusResponse response = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
            if (blockInfo.IpAddress.Count() == 0)
            {
                response.Type = ClientProto.StatusResponse.Types.StatusType.Ready;
                return Task.FromResult(response);
            }
            else
            {
                Guid blockId = Guid.Parse(blockInfo.BlockId.Value);
                Channel channel = null;
                string ipAddress = "";
                try
                {
                    ipAddress = blockInfo.IpAddress[0];
                    blockInfo.IpAddress.RemoveAt(0);
                    channel = ConnectionManager.Instance.CreateChannel(blockId, "127.0.0.1", "50052");
                    //Console.WriteLine(channel.State.ToString());
                    //Channel channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);
                    var client = new ClientProto.ClientProto.ClientProtoClient(channel);
                    //return client.GetReady(new ClientProto.DataNodeAddresses { IpAddress = { addresses } });

                    client.GetReady(blockInfo);
                }
                catch (RpcException e)
                {
                    ConnectionManager.Instance.ShutDownChannel(blockId, channel);
                    response.Message = "Failed to get ready for " + ipAddress + ": " + e.Message;
                }

                // We will always return ready as if it reaches this far there is at least 1 node in the pipeline, which is good enough!
                return Task.FromResult(response);
            }
        }

        /// <summary>
        /// Server side handler of the WriteDataBlock RPC. DataNode writes block to a file and then returns success status of write.
        /// </summary>
        /// <param name="requestStream">Stream of bytes</param>
        /// <param name="context">Context of call. Contains metadata containing blockid and list of addresses</param>
        /// <returns>Status of task</returns>
        public override async Task<ClientProto.StatusResponse> WriteBlock(Grpc.Core.IAsyncStreamReader<ClientProto.BlockData> requestStream, ServerCallContext context)
        {
            List<Metadata.Entry> metaData = context.RequestHeaders.ToList();

            // Get blockID
            Guid blockId = GetBlockID(metaData);

            //TODO: switch this
            //string filePath = BlockStorage.Instance.CreateFile(blockId);
            string filePath = BlockStorage.Instance.CreateFile(Guid.NewGuid());
            Console.WriteLine("Created file: " + filePath);

            Channel channel = ConnectionManager.Instance.GetChannel(blockId);

            // No channel found means last datanode in pipe
            if (channel != null)
            {
                return await WriteAndForwardBlock(requestStream, context, channel, filePath, blockId);
            }
            else // Just write to file
            {
                Console.WriteLine("Last Datanode in pipe");
                return await WriteBlock(requestStream, filePath, blockId);
            }
        }

        /// <summary>
        /// Writes block to disk and forwards to next DataNode
        /// </summary>
        /// <param name="requestStream">Stream of bytes</param>
        /// <param name="context">Context of call</param>
        /// <param name="channel">Channel for pipe</param>
        /// <param name="filePath">Full path of file</param>
        /// <param name="blockId">Unique identifier of block</param>
        /// <returns>Status of writing the block</returns>
        public static async Task<ClientProto.StatusResponse> WriteAndForwardBlock(Grpc.Core.IAsyncStreamReader<ClientProto.BlockData> requestStream, ServerCallContext context, Channel channel, string filePath, Guid blockId)
        {
            bool success = true;
            string message = "";
            var client = new ClientProto.ClientProto.ClientProtoClient(channel);
            using (var call = client.WriteBlock(context.RequestHeaders))
            {
                bool dataNodeFailed = false;
                Stopwatch watch = new Stopwatch();
                watch.Start();

                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 131072, FileOptions.WriteThrough)) // 128KB
                {
                    while (await requestStream.MoveNext())
                    {
                        try
                        {
                            var blockData = requestStream.Current;
                            byte[] data = blockData.Data.ToByteArray();

                            // Write to file
                            stream.Write(data, 0, data.Length);

                            // Don't need to forward data in pipe if datanode failed
                            if (!dataNodeFailed)
                            {
                                try
                                {
                                    // Send data through pipe
                                    await call.RequestStream.WriteAsync(blockData);
                                }
                                catch(RpcException e)
                                {
                                    dataNodeFailed = true;
                                    Console.WriteLine("Writing block failed: " + e.Message);
                                    message = e.Message;
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            message = e.Message;
                            success = false;
                        }
                    }
                    stream.Flush();
                    stream.Dispose();
                }

                ClientProto.StatusResponse resp = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail, Message = message };

                // If DataNode did not fail, get response and shut down
                if (!dataNodeFailed)
                {
                    await call.RequestStream.CompleteAsync();

                    resp = await call.ResponseAsync;
                    call.Dispose();
                    ConnectionManager.Instance.ShutDownChannel(blockId, channel);
                }

                watch.Stop();
                Console.WriteLine("Total time to write: " + watch.Elapsed);

                // If write was successful and block size is correct, return success
                // Otherwise return the response sent down through pipe
                return (success && BlockStorage.Instance.ValidateBlock(blockId, filePath)) ? new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success } : resp;
            }
        }

        /// <summary>
        /// Writes block to disk
        /// </summary>
        /// <param name="requestStream">Stream of bytes</param>
        /// <param name="filePath">Full path of file</param>
        /// <param name="blockId">Unique identifier of block</param>
        /// <returns>Status of writing the block</returns>
        public static async Task<ClientProto.StatusResponse> WriteBlock(Grpc.Core.IAsyncStreamReader<ClientProto.BlockData> requestStream, string filePath, Guid blockId)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 131072, FileOptions.WriteThrough)) // 128KB
            {
                while (await requestStream.MoveNext())
                {
                    try
                    {
                        var blockData = requestStream.Current;
                        byte[] data = blockData.Data.ToByteArray();

                        // Write to file
                        stream.Write(data, 0, data.Length);
                    }
                    catch (IOException e)
                    {
                        BlockStorage.Instance.DeleteBlock(blockId);
                        return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail, Message = e.Message };
                    }
                }

                watch.Stop();
                Console.WriteLine("Total time to write: " + watch.Elapsed);
                stream.Flush();
                stream.Dispose();
            }

            // If write was successful, make sure block size is correct
            return !BlockStorage.Instance.ValidateBlock(blockId, filePath) ? new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail, Message = "Block size is not correct" } : new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
        }

        /// <summary>
        /// Gets blockID from list of metadata
        /// </summary>
        /// <param name="metaData">List of metadata</param>
        /// <returns>BlockID</returns>
        public static Guid GetBlockID(List<Metadata.Entry> metaData)
        {
            Guid id = new Guid();
            try
            {
                Metadata.Entry blckId = metaData.Find(m => { return m.Key == "blockid"; });
                id = Guid.Parse(blckId.Value);
                return id;
            }
            catch
            {
                return id;
            }
        }
    }
}
