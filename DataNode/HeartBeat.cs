using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    public static class HeartBeat
    {
        /// <summary>
        /// Send blockreport
        /// </summary>
        /// <param name="client">Datanode grpc client</param>
        public static async Task SendHeartBeat(DataNodeProto.DataNodeProto.DataNodeProtoClient client)
        {
            while (true)
            {
                DataNodeProto.HeartBeatRequest heartBeatRequest = CreateHeartBeatRequest();

                try
                {
                    DataNodeProto.HeartBeatResponse response = client.SendHeartBeat(heartBeatRequest);

                    Console.WriteLine("heart beat response: " + response.ToString());
                    DataNodeProto.BlockCommand nameNodeCommands = response.Commands;
                    switch (nameNodeCommands.Action)
                    {
                        case DataNodeProto.BlockCommand.Types.Action.Transfer:
                            Console.WriteLine("Transfer action!!!");
                            foreach (var block in nameNodeCommands.DataBlock.ToList())
                            {
                                // Get block data
                                byte[] blockData = BlockStorage.Instance.ReadBlock(Guid.Parse(block.BlockId.Value));

                                if (blockData != null)
                                {
                                    Metadata metaData = new Metadata {
                                            new Metadata.Entry("blockid", block.BlockId.Value),
                                            new Metadata.Entry("blocksize", blockData.Length.ToString())
                                        };

                                    // Send data to each block
                                    foreach (var dataNode in block.DataNodes)
                                    {
                                        Task task = ForwardBlock(dataNode, blockData, metaData);
                                    }
                                }
                            }
                            break;
                        case DataNodeProto.BlockCommand.Types.Action.Delete:
                            Console.WriteLine("Delete action");
                            InvalidateBlocks(nameNodeCommands.BlockList);
                            break;
                    }
                }
                catch (RpcException e)
                {
                    Console.WriteLine("HeartBeat failed: " + e.Message);
                }
                await Task.Delay(Constants.HeartBeatInterval); // This is an HDFS default
            }
        }

        /// <summary>
        /// Creates a heartbeat
        /// </summary>
        /// <returns>Heart beat request message</returns>
        public static DataNodeProto.HeartBeatRequest CreateHeartBeatRequest()
        {
            return new DataNodeProto.HeartBeatRequest
            {
                NodeInfo = new DataNodeProto.DataNodeInfo
                {
                    DataNode = new DataNodeProto.DataNode
                    {
                        IpAddress = Program.ipAddress
                    },
                    DiskSpace = BlockStorage.Instance.GetFreeDiskSpace()
                }
            };
        }

        /// <summary>
        /// Function to loop through each blockid sent from namenode and delete the files
        /// </summary>
        /// <param name="blockList">List of blockIds to delete</param>
        public static void InvalidateBlocks(DataNodeProto.BlockList blockList)
        {
            List<DataNodeProto.UUID> blockIds = blockList.BlockId.ToList();
            foreach (var id in blockIds)
            {
                BlockStorage.Instance.DeleteBlock(Guid.Parse(id.Value));
            }
        }

        public static async Task ForwardBlock(DataNodeProto.DataNode dataNode, byte[] blockData, Metadata metadata)
        {
            try
            {
                Console.WriteLine("Fowarding block to " + dataNode.IpAddress);
                Channel channel = new Channel(dataNode.IpAddress + ":" + Constants.Port, ChannelCredentials.Insecure);
                var nodeClient = new DataNodeProto.DataNodeProto.DataNodeProtoClient(channel);

                using (var call = nodeClient.WriteDataBlock(metadata))
                {
                    int remaining = blockData.Length;

                    while (remaining > 0)
                    {
                        var copyLength = Math.Min(Constants.StreamChunkSize, remaining);
                        byte[] streamBuffer = new byte[copyLength];

                        Buffer.BlockCopy(
                            blockData,
                            blockData.Length - remaining,
                            streamBuffer, 0,
                            copyLength);

                        await call.RequestStream.WriteAsync(new DataNodeProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(streamBuffer) });

                        remaining -= copyLength;
                    }

                    await call.RequestStream.CompleteAsync();
                    var resp = await call.ResponseAsync;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Problem forwarding block to node," + e);
            }
        }
    }
}