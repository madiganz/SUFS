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
        public static async void SendHeartBeat(object client)
        {
            DataNodeProto.DataNodeProto.DataNodeProtoClient tClient = ((DataNodeProto.DataNodeProto.DataNodeProtoClient)client);

            while (true)
            {
                DataNodeProto.HeartBeatRequest heartBeatRequest = new DataNodeProto.HeartBeatRequest
                {
                    NodeInfo = new DataNodeProto.DataNodeInfo
                    {
                        DataNode = new DataNodeProto.DataNode
                        {
                            Id = new DataNodeProto.UUID { Value = Program.mNodeID.ToString() },
                            IpAddress = Program.ipAddress
                        },
                        DiskSpace = BlockStorage.Instance.GetFreeDiskSpace()
                    }
                };

                DataNodeProto.HeartBeatResponse response = tClient.SendHeartBeat(heartBeatRequest, new CallOptions().WithWaitForReady(true));
                Console.WriteLine(response);

                List<DataNodeProto.DataNodeCommands> nameNodeCommands = response.Commands.ToList();

                foreach (var command in nameNodeCommands)
                {
                    // TODO: DEAL WITH THIS?
                    //switch(command.)
                    //{
                    //    case DataNodeProto.DataNodeCommands.Types.Type.BlockCommand:
                    //        break;
                    //    case DataNodeProto.DataNodeCommands.Types.Type.BlockRecoveryCommand:
                    //        break;
                    //}

                    switch (command.Command.Action)
                    {
                        case DataNodeProto.BlockCommand.Types.Action.Transfer:
                            foreach (var block in command.Command.DataBlock.ToList())
                            {   
                                // Get block data
                                byte[] blockData = BlockStorage.Instance.ReadBlock(Guid.Parse(block.BlockId.Value));

                                // Convert byte[] to ByteString
                                block.Data = Google.Protobuf.ByteString.CopyFrom(blockData);

                                // Send data to each block
                                foreach( var dataNode in block.DataNodes)
                                {
                                    Channel channel = new Channel(dataNode.IpAddress + ":" + Constants.Port, ChannelCredentials.Insecure);
                                    var nodeClient = new DataNodeProto.DataNodeProto.DataNodeProtoClient(channel);
                                    nodeClient.WriteDataBlockAsync(block); // TODO: Need to wait????
                                    //channel.ShutdownAsync(); // NEED THIS?
                                }
                            }
                            break;
                        case DataNodeProto.BlockCommand.Types.Action.Delete:
                            InvalidateBlocks(command.Command.BlockList);
                            break;
                    }
                }
                await Task.Delay(Constants.HeartBeatInterval); // This is an HDFS default
            }
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
