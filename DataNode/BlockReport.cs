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
        /// Sends block report
        /// </summary>
        /// <param name="client">Datanode grpc client</param>
        public static async void SendBlockReport(object client)
        {
            DataNodeProto.DataNodeProto.DataNodeProtoClient tClient = ((DataNodeProto.DataNodeProto.DataNodeProtoClient)client);

            while (true)
            {
                DataNodeProto.BlockReportRequest blockReport = CreateBlockReport();
                DataNodeProto.StatusResponse response = tClient.SendBlockReport(blockReport);
                Console.WriteLine(response.Type.ToString());
                await Task.Delay(20000);
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
                NodeId = new DataNodeProto.UUID
                {
                    Value = Program.mNodeID.ToString()
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
