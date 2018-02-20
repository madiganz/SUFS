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
        /// <summary>
        /// Server side handler of the WriteBlock RPC. DataNode writes block to a file and then passes block to next DataNode in list.
        /// Waits until that node's response before responding to the Client / previous DataNode.
        /// </summary>
        /// <param name="blockContainer">Containing holding blockid, data, and nodes to forward to</param>
        /// <param name="context">Call Context</param>
        /// <returns></returns>
        //public override Task<ClientProto.StatusResponse> WriteBlock(ClientProto.DataBlock blockContainer, ServerCallContext context)
        //{
        //    context.RequestHeaders.
        //    Console.WriteLine(Encoding.Default.GetString(blockContainer.Data.ToByteArray()));

        //    // Write block to file system
        //    bool writeSuccess = BlockStorage.Instance.WriteBlock(Guid.Parse(blockContainer.BlockId.Value), blockContainer.Data.ToByteArray());

        //    // If last block in list, just return status
        //    if (blockContainer.DataNodes.Count == 0)
        //    {
        //        var resp = writeSuccess ? ClientProto.StatusResponse.Types.StatusType.Success : ClientProto.StatusResponse.Types.StatusType.Fail;
        //        return Task.FromResult(new ClientProto.StatusResponse { Type = resp });
        //    }

        //    // Get first node in list
        //    ClientProto.DataNode nodeToFoward = blockContainer.DataNodes[0];

        //    // Remove that node from list
        //    blockContainer.DataNodes.RemoveAt(0);

        //    // Need to connect to the nodes ip address
        //    Channel channel = new Channel(nodeToFoward.IpAddress + ":" + Constants.Port, ChannelCredentials.Insecure);

        //    var client = new ClientProto.ClientProto.ClientProtoClient(channel);
        //    ClientProto.StatusResponse nodeResponse = client.WriteBlock(blockContainer);

        //    bool forwardSuccess = nodeResponse.Type == ClientProto.StatusResponse.Types.StatusType.Success ? true : false;

        //    ClientProto.StatusResponse.Types.StatusType response;
        //    if (writeSuccess && forwardSuccess)
        //    {
        //        response = ClientProto.StatusResponse.Types.StatusType.Success;
        //    }
        //    else
        //    {
        //        response = ClientProto.StatusResponse.Types.StatusType.Fail;
        //    }

        //    return Task.FromResult(new ClientProto.StatusResponse { Type = response });
        //}

        public override async Task<ClientProto.StatusResponse> WriteBlock(Grpc.Core.IAsyncStreamReader<ClientProto.BlockData> requestStream, ServerCallContext context)
        {
            List<Metadata.Entry> metaData = context.RequestHeaders.ToList();
            Metadata.Entry ipAddress = null;
            bool forward = true;
            try
            {
                ipAddress = metaData.Find(m => { return m.Key == "ipaddresses"; });
                if(ipAddress == null)
                {
                    forward = false;
                }
            }
            catch (ArgumentNullException)
            {
                forward = false;
            }
            Metadata.Entry blockId = metaData.Find(m => { return m.Key == "blockid"; });
            string filePath = BlockStorage.Instance.CreateFile(Guid.Parse(blockId.Value));
            Console.WriteLine("Just created file: " + filePath);

            // Forward to next datanode
            if (forward)
            {
                Console.WriteLine("INSIDE OF FORWARD");
                List<string> addresses = ipAddress.Value.Split(',').ToList();
                var ip = addresses[0];
                addresses.RemoveAt(0);
                Channel channel = new Channel(ip + ":" + Constants.Port, ChannelCredentials.Insecure);
                var client = new ClientProto.ClientProto.ClientProtoClient(channel);
                string nodeAddresses = "";
                Metadata newMetaData = new Metadata
                    {
                        new Metadata.Entry("blockid", blockId.Value)
                    };

                if (addresses.Count > 0)
                {
                    nodeAddresses = String.Join(",", addresses.ToArray());
                    newMetaData.Add(new Metadata.Entry("ipaddresses", nodeAddresses));
                }
                using (var call = client.WriteBlock(newMetaData))
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    Console.WriteLine("Writing Block to " + filePath);
                    using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 131072, FileOptions.WriteThrough)) // 128KB
                    {
                        while (await requestStream.MoveNext())
                        {
                            try
                            {
                                var blockData = requestStream.Current;
                                byte[] data = blockData.Data.ToByteArray();
                                stream.Write(data, 0, data.Length);
                                await call.RequestStream.WriteAsync(blockData);
                                //await call.RequestStream.CompleteAsync();

                                //ClientProto.StatusResponse resp = await call.ResponseAsync;
                                //if (resp.Type == ClientProto.StatusResponse.Types.StatusType.Fail)
                                //{
                                //    return resp;
                                //}
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine(e);
                                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                            }
                        }

                        await call.RequestStream.CompleteAsync();

                        ClientProto.StatusResponse resp = await call.ResponseAsync;
                        if (resp.Type == ClientProto.StatusResponse.Types.StatusType.Fail)
                        {
                            return resp;
                        }

                        watch.Stop();
                        Console.WriteLine("Total time to write: " + watch.Elapsed);
                    }

                    return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
                }
            }
            else // Just write to file
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Console.WriteLine("Writing Block to " + filePath);
                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 131072, FileOptions.WriteThrough)) // 128KB
                {
                    while (await requestStream.MoveNext())
                    {
                        try
                        {
                            var blockData = requestStream.Current;
                            byte[] data = blockData.Data.ToByteArray();
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

                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
            }
        }
    }
}
