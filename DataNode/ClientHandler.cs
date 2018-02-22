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
        /// Server side handler of the WriteDataBlock RPC. DataNode writes block to a file and then returns success status of write.
        /// </summary>
        /// <param name="requestStream">Stream of bytes</param>
        /// <param name="context">Context of call. Contains metadata containing blockid and list of addresses</param>
        /// <returns>Status of task</returns>
        public override async Task<ClientProto.StatusResponse> WriteBlock(Grpc.Core.IAsyncStreamReader<ClientProto.BlockData> requestStream, ServerCallContext context)
        {
            List<Metadata.Entry> metaData = context.RequestHeaders.ToList();
            Metadata.Entry ipAddress = null;
            bool forward = true;
            bool success = true;
            try
            {
                ipAddress = metaData.Find(m => { return m.Key == "ipaddresses"; });
                if (ipAddress == null)
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
            //string filePath = BlockStorage.Instance.CreateFile(Guid.NewGuid());
            Console.WriteLine("Just created file: " + filePath);

            // Forward to next datanode
            if (forward)
            {
                List<string> addresses = ipAddress.Value.Split(',').ToList();
                var ip = addresses[0];
                addresses.RemoveAt(0);
                Channel channel = new Channel(ip + ":" + Constants.Port, ChannelCredentials.Insecure);
                //Channel channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);
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
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine(e);
                                success = false;
                            }
                        }
                    }

                    await call.RequestStream.CompleteAsync();

                    ClientProto.StatusResponse resp = await call.ResponseAsync;
                    watch.Stop();
                    Console.WriteLine("Total time to write: " + watch.Elapsed);
                    channel.ShutdownAsync().Wait();

                    // Return success if any writes were successful
                    if (success || (resp.Type == ClientProto.StatusResponse.Types.StatusType.Success))
                    {
                        return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
                    }
                    else
                    {
                        return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                    }
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
