using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="requestStream">Data of block</param>
        /// <param name="context">Call Context</param>
        /// <returns>Status of task</returns>
        public override async Task<DataNodeProto.StatusResponse> WriteDataBlock(Grpc.Core.IAsyncStreamReader<DataNodeProto.BlockData> requestStream, ServerCallContext context)
        {
            List<Metadata.Entry> metaData = context.RequestHeaders.ToList();
            Guid blockId = Util.GetBlockID(metaData);
            int blockSize = Util.GetBlockSize(metaData);

            Console.WriteLine("Writing block data sent from datanode");
            Console.WriteLine("BlockID = " + blockId);

            string filePath = BlockStorage.Instance.CreateFile(blockId);

            bool writeSuccess = true;

            // Write block to file system
            using (var writerStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                while (await requestStream.MoveNext())
                {
                    try
                    {
                        var bytes = requestStream.Current.Data.ToByteArray();
                        writerStream.Write(bytes, 0, bytes.Length);
                    }
                    catch
                    {
                        writeSuccess = false;
                    }
                }
            }

            if (BlockStorage.Instance.ValidateBlock(blockId, filePath, blockSize) && writeSuccess)
            {
                // Send block report to NameNode
                var client = new DataNodeProto.DataNodeProto.DataNodeProtoClient(ConnectionManager.Instance.NameNodeConnection);
                Console.WriteLine("Write success from a forward! sending single block report!");
                BlockReport.SendSingleBlockReport(client);
                return new DataNodeProto.StatusResponse { Type = DataNodeProto.StatusResponse.Types.StatusType.Success };
            }
            else
            {
                return new DataNodeProto.StatusResponse { Type = DataNodeProto.StatusResponse.Types.StatusType.Fail };
            }
        }
    }
}
