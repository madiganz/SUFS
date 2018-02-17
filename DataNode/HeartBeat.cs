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
                DataNodeProto.HeartBeatRequest heartBeatRequest = new DataNodeProto.HeartBeatRequest { HeartBeatString = "I AM A HEARTBEAT" };
                DataNodeProto.HeartBeatResponse response = tClient.SendHeartBeat(heartBeatRequest);
                Console.WriteLine(response);

                List<DataNodeProto.DataNodeCommands> nameNodeCommands = response.Cmds.ToList();

                foreach(var command in nameNodeCommands)
                {
                    // TODO: DEAL WITH THIS?
                    switch(command.CmdType)
                    {
                        case DataNodeProto.DataNodeCommands.Types.Type.BlockCommand:
                            break;
                        case DataNodeProto.DataNodeCommands.Types.Type.BlockRecoveryCommand:
                            break;
                    }

                    switch(command.BlkCmd.Action)
                    {
                        case DataNodeProto.BlockCommandProto.Types.Action.Transfer:
                            tClient.WriteDataBlockAsync(command.BlkCmd.DataBlock); // TODO: Need to wait????
                            break;
                        case DataNodeProto.BlockCommandProto.Types.Action.Invalidate:
                            InvalidateBlocks(command.BlkCmd.BlockList);
                            break;
                    }
                }
                await Task.Delay(3000); // This is an HDFS default
            }
        }

        /// <summary>
        /// Function to loop through each blockid sent from namenode and delete the files
        /// </summary>
        /// <param name="blockList">List of blockIds to delete</param>
        public static void InvalidateBlocks(DataNodeProto.BlockList blockList)
        {
            List<DataNodeProto.UUID> blockIds = blockList.BlockId.ToList();
            foreach(var id in blockIds)
            {
                BlockStorage.Instance.DeleteBlock(Guid.Parse(id.Value));
            }
        }
    }
}
