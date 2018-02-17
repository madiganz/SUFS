using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    public static class BlockReport
    {
        public static async void SendBlockReport(object client)
        {
            //Task.Run(async () =>
            //{
                while (true)
                {
                    DataNodeProto.BlockReportrequest blockReport = CreateBlockReport();
                    DataNodeProto.StatusResponse response = ((DataNodeProto.DataNodeService.DataNodeServiceClient)client).SendBlockReport(blockReport);
                    Console.WriteLine(response.Type.ToString());
                    await Task.Delay(20000);
                }
            //});
        }

        /// <summary>
        /// Block report is the list of block ids contained in blockstorage
        /// </summary>
        public static DataNodeProto.BlockReportrequest CreateBlockReport()
        {
            //DataNodeProto.BlockReportrequest blockReportRequest = new DataNodeProto.BlockReportrequest();
            Guid[] blockList = BlockStorage.GetBlocks();
            //blockReportRequest.Nodeid = new DataNodeProto.UUID { Value = Program.mNodeID.ToString() };
            List<DataNodeProto.UUID> blockIdList = new List<DataNodeProto.UUID>();
            for (int i = 0; i < blockList.Length; i++)
            {
                blockIdList.Add(new DataNodeProto.UUID { Value = blockList[i].ToString() });
            }
            //blockReportRequest.Blocklist = new DataBlock { Blockid = blockIdList.Where()};
            DataNodeProto.BlockReportrequest blockReportRequest = new DataNodeProto.BlockReportrequest
            {
                Nodeid = new DataNodeProto.UUID
                {
                    Value = Program.mNodeID.ToString()
                },
                Blocklist = new DataNodeProto.BlockList
                {
                    Blockid = { blockIdList }
                }
            };
            return blockReportRequest;
        }
    }

    /// <summary>
    /// Structure of the block report to be sent
    /// </summary>
    //public class BlockReportStructure
    //{
    //    private int nodeid { get; set; }
    //    private Guid[] blocks { get; set; }

    //    public BlockReportStructure(int id, Guid[] blockList)
    //    {
    //        nodeid = id;
    //        blocks = blockList;
    //    }

    //    public override string ToString()
    //    {
    //        return "node id = " + nodeid + ", number of blocks = " + blocks.Length;
    //    }
    //}
}
