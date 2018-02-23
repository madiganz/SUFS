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

        public override Task<ClientProto.StatusResponse> GetReady(ClientProto.DataNodeAddresses addresses, ServerCallContext context)
        {
            ClientProto.StatusResponse response = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            if (addresses.IpAddress.Count() == 0)
            {
                response.Type = ClientProto.StatusResponse.Types.StatusType.Ready;
                return Task.FromResult(response);
            }
            else
            {
                try
                {
                    string address = addresses.IpAddress[0];
                    addresses.IpAddress.RemoveAt(0);
                    Channel channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);
                    Console.WriteLine(channel.State.ToString());
                    var client = new ClientProto.ClientProto.ClientProtoClient(channel);
                    //return client.GetReady(new ClientProto.DataNodeAddresses { IpAddress = { addresses } });

                    response = client.GetReady(addresses);
                }
                catch (RpcException e)
                {
                    //Console.WriteLine(e);
                    Console.WriteLine("failed");
                }

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
            bool forward = true;
            bool success = true;

            // Get blockID
            Guid blockId = GetBlockID(metaData);

            // Get IpAddresses
            Metadata.Entry ipAddresses = GetIpAddresses(metaData);

            // If no addresses -> last DataNode
            if (ipAddresses == null)
            {
                forward = false;
            }

            //string filePath = BlockStorage.Instance.CreateFile(blockId);
            string filePath = BlockStorage.Instance.CreateFile(Guid.NewGuid());
            Console.WriteLine("Created file: " + filePath);

            // Forward to next datanode
            if (forward)
            {
                List<string> addresses = ipAddresses.Value.Split(',').ToList();

                // Get next address and remove from list
                string ip = GetNextIpAddress(addresses);

                // Modify metadata using modified list of addresses
                Metadata modifiedMetaData = ModifyMetaData(addresses, blockId);

                //Channel channel = new Channel(ip + ":" + Constants.Port, ChannelCredentials.Insecure);
                Channel channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);
                var client = new ClientProto.ClientProto.ClientProtoClient(channel);

                using (var call = client.WriteBlock(modifiedMetaData))
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
                                    catch
                                    {
                                        dataNodeFailed = true;
                                        Console.WriteLine("Writing block failed");
                                    }
                                }
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine(e);
                                success = false;
                            }
                        }
                    }

                    ClientProto.StatusResponse resp = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };

                    // If DataNode did not fail, get response and shut down
                    if (!dataNodeFailed)
                    {
                        await call.RequestStream.CompleteAsync();

                        resp = await call.ResponseAsync;

                        channel.ShutdownAsync().Wait();
                    }

                    watch.Stop();
                    Console.WriteLine("Total time to write: " + watch.Elapsed);

                    // If write was successful and block size is correct, return success
                    if (success && BlockStorage.Instance.ValidateBlock(blockId, filePath))
                    {
                        return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
                    }
                    else // If not successful, return the response sent down through pipe
                    {
                        return resp;
                    }
                    
                }
            }
            else // Just write to file
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
                            Console.WriteLine(e);
                            return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                        }
                    }

                    watch.Stop();
                    Console.WriteLine("Total time to write: " + watch.Elapsed);
                }

                // If write was successful, make sure block size is correct
                if (!BlockStorage.Instance.ValidateBlock(blockId, filePath))
                {
                    return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                }
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
            }
        }

        /// <summary>
        /// Gets ip address of next DataNode
        /// </summary>
        /// <param name="addresses">List of ip addresses</param>
        /// <returns>Next DataNode's ip address</returns>
        public static string GetNextIpAddress(List<string> addresses)
        {
            var ip = addresses[0];
            addresses.RemoveAt(0);
            return ip;
        }

        /// <summary>
        /// Modify metadata based on remaining ip addresses
        /// </summary>
        /// <param name="addresses">List of remaining ip addresses</param>
        /// <param name="blockId">Unique idenfier of block</param>
        /// <returns>Modified metadata</returns>
        public static Metadata ModifyMetaData(List<string> addresses, Guid blockId)
        {
            Metadata metaData = new Metadata { new Metadata.Entry("blockid", blockId.ToString()) };

            if (addresses.Count > 0)
            {
                metaData.Add(new Metadata.Entry("ipaddresses", String.Join(",", addresses.ToArray())));
            }

            return metaData;
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

        public static Metadata.Entry GetIpAddresses(List<Metadata.Entry> metaData)
        {
            try
            {
                return metaData.Find(m => { return m.Key == "ipaddresses"; });
            }
            catch
            {
                return null;
            }
        }
    }
}
