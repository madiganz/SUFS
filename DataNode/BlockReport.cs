using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    public static class BlockReport
    {
        /// <summary>
        /// Sends block report in loop
        /// </summary>
        /// <param name="client">Datanode grpc client</param>
        public static async Task SendBlockReport(DataNodeProto.DataNodeProto.DataNodeProtoClient client)
        {
            while (true)
            {
                SendSingleBlockReport(client);

                await Task.Delay(Constants.BlockReportInterval);
            }
        }

        /// <summary>
        /// Sends a single block report
        /// </summary>
        /// <param name="client">Connection to NameNode</param>
        public static void SendSingleBlockReport(DataNodeProto.DataNodeProto.DataNodeProtoClient client)
        {
            try
            {
                DataNodeProto.BlockReportRequest blockReport = CreateBlockReport();
                DataNodeProto.StatusResponse response = client.SendBlockReport(blockReport);
            }
            catch (RpcException e)
            {
                Console.WriteLine("Blockreport failed: " + e.Message);
            }
        }

        /// <summary>
        /// Block report is the list of block ids contained in blockstorage
        /// </summary>
        public static DataNodeProto.BlockReportRequest CreateBlockReport()
        {
            Guid[] blockList = BlockStorage.GetBlocks();
            List<DataNodeProto.UUID> blockIdList = new List<DataNodeProto.UUID>();
            for (int i = 0; i < blockList.Length; i++)
            {
                blockIdList.Add(new DataNodeProto.UUID { Value = blockList[i].ToString() });
            }

            DataNodeProto.BlockReportRequest BlockReportRequest = new DataNodeProto.BlockReportRequest
            {
                DataNode = new DataNodeProto.DataNode
                {
                    IpAddress = Program.ipAddress
                },
                BlockList = new DataNodeProto.BlockList
                {
                    BlockId = { blockIdList }
                }
            };
            return BlockReportRequest;
        }
    }
}
