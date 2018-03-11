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


                    List<DataNodeProto.BlockCommand> nameNodeCommands = response.Commands.ToList();

                    foreach (var command in nameNodeCommands)
                    {
                        switch (command.Action)
                        {
                            case DataNodeProto.BlockCommand.Types.Action.Transfer:
                                foreach (var block in command.DataBlock.ToList())
                                {
                                    // Get block data
                                    byte[] blockData = BlockStorage.Instance.ReadBlock(Guid.Parse(block.BlockId.Value));

                                    // Convert byte[] to ByteString
                                    block.Data = Google.Protobuf.ByteString.CopyFrom(blockData);

                                    // Send data to each block
                                    foreach (var dataNode in block.DataNodes)
                                    {
                                        Channel channel = new Channel(dataNode.IpAddress + ":" + Constants.Port, ChannelCredentials.Insecure);
                                        var nodeClient = new DataNodeProto.DataNodeProto.DataNodeProtoClient(channel);
                                        await nodeClient.WriteDataBlockAsync(block);
                                        await channel.ShutdownAsync();
                                    }
                                }
                                break;
                            case DataNodeProto.BlockCommand.Types.Action.Delete:
                                InvalidateBlocks(command.BlockList);
                                break;
                        }
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
    }
}