using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameNode
{
    class DataNodeHandler : DataNodeProto.DataNodeProto.DataNodeProtoBase
    {
        // Server side handler of the SendBlockReportRequest RPC
        public override Task<DataNodeProto.StatusResponse> SendBlockReport(DataNodeProto.BlockReportRequest request, ServerCallContext context)
        {
            var blockListLength = request.BlockList.BlockId.Count();
            Guid[] blockList = new Guid[blockListLength];
            for (var i = 0; i < blockListLength; i++)
            {
                blockList[i] = Guid.Parse(request.BlockList.BlockId[i].Value);
            }
            Console.WriteLine("Block report from datanode: " + blockList.ToString());
            return Task.FromResult(DataNodeManager.Instance.ProcessBlockReport(blockList, request.DataNode.IpAddress));
        }

        // Server side handler of the SendHeartBeat RPC
        public override Task<DataNodeProto.HeartBeatResponse> SendHeartBeat(DataNodeProto.HeartBeatRequest request, ServerCallContext context)
        {
            DataNodeProto.BlockCommand blockCommands = DataNodeManager.Instance.UpdateDataNodes(request.NodeInfo);
            Console.WriteLine("Commands to be sent back to datanode: " + blockCommands.ToString());
            Console.WriteLine("Block command action: " + blockCommands.Action.ToString());
            DataNodeProto.HeartBeatResponse response = new DataNodeProto.HeartBeatResponse
            {
                Commands = blockCommands
            };

            return Task.FromResult(response);
        }
    }
}