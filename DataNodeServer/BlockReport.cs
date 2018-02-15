using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNodeServer
{
    public static class BlockReport
    {
        public static void SendBlockReport()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    BlockReportStructure blockReport = CreateBlockReport();
                    Console.WriteLine("I AM A BLOCKREPORT: " + blockReport.ToString());
                    await Task.Delay(20000);
                }
            });
        }

        /// <summary>
        /// Block report is the list of block ids contained in blockstorage
        /// </summary>
        public static BlockReportStructure CreateBlockReport()
        {
            Guid[] blockList = BlockStorage.GetBlocks();

            return new BlockReportStructure(1, blockList);
        }
    }

    /// <summary>
    /// Structure of the block report to be sent
    /// </summary>
    public class BlockReportStructure
    {
        private int nodeid { get; set; }
        private Guid[] blocks { get; set; }

        public BlockReportStructure(int id, Guid[] blockList)
        {
            nodeid = id;
            blocks = blockList;
        }

        public override string ToString()
        {
            return "node id = " + nodeid + ", number of blocks = " + blocks.Length;
        }
    }
}
