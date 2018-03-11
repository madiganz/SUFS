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
            return Task.FromResult(new DataNodeProto.StatusResponse { Type = DataNodeProto.StatusResponse.Types.StatusType.Success });
        }

        // Server side handler of the SendHeartBeat RPC
        public override Task<DataNodeProto.HeartBeatResponse> SendHeartBeat(DataNodeProto.HeartBeatRequest request, ServerCallContext context)
        {            List<DataNodeProto.BlockCommand> blockCommands = DataNodeManager.Instance.UpdateDataNodes(request.NodeInfo);

            // TODO: Use the redistribution logic
            // Create fake list of blocks to invalidate
            //List<DataNodeProto.UUID> blocks = new List<DataNodeProto.UUID>();
            //for (var i = 0; i < 11; i++)
            //{
            //    blocks.Add(new DataNodeProto.UUID { Value = Guid.NewGuid().ToString() });
            //}

            //// Create command
            //DataNodeProto.DataNodeCommands commands = new DataNodeProto.DataNodeCommands
            //{
            //    Command = new DataNodeProto.BlockCommand
            //    {
            //        Action = DataNodeProto.BlockCommand.Types.Action.Delete,
            //        BlockList = new DataNodeProto.BlockList
            //        {
            //            BlockId = { blocks }
            //        }
            //    }

            //};



            DataNodeProto.HeartBeatResponse response = new DataNodeProto.HeartBeatResponse
            {
                Commands = { blockCommands }
            };

            blockCommands.Clear();

            return Task.FromResult(response);
        }
    }
}
