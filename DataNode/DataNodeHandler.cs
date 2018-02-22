using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    class DataNodeHandler : DataNodeProto.DataNodeProto.DataNodeProtoBase
    {
        /// <summary>
        /// Server side handler of the WriteDataBlock RPC. DataNode writes block to a file and then returns success status of write.
        /// </summary>
        /// <param name="blockContainer">Containing holding blockid, data, and nodes to forward to</param>
        /// <param name="context">Call Context</param>
        /// <returns>Status of task</returns>
        public override Task<DataNodeProto.StatusResponse> WriteDataBlock(DataNodeProto.DataBlock blockContainer, ServerCallContext context)
        {
            Console.WriteLine(Encoding.Default.GetString(blockContainer.Data.ToByteArray()));

            string filePath = BlockStorage.Instance.CreateFile(Guid.Parse(blockContainer.BlockId.Value));

            // Write block to file system
            bool writeSuccess = BlockStorage.Instance.WriteBlock(Guid.Parse(blockContainer.BlockId.Value), filePath, blockContainer.Data.ToByteArray());

            var resp = writeSuccess ? DataNodeProto.StatusResponse.Types.StatusType.Success : DataNodeProto.StatusResponse.Types.StatusType.Fail;
            return Task.FromResult(new DataNodeProto.StatusResponse { Type = resp });
        }
    }
}
