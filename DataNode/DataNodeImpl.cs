using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    class DataNodeImpl : DataNodeProto.DataNodeProto.DataNodeProtoBase
    {
        /// <summary>
        /// Server side handler of the WriteDataBlock RPC. DataNode writes block to a file and then passes block to next DataNode in list.
        /// Waits until that node's response before responding to the Client / previous DataNode.
        /// </summary>
        /// <param name="blockContainer">Containing holding blockid, data, and nodes to forward to</param>
        /// <param name="context">Call Context</param>
        /// <returns></returns>
        public override Task<DataNodeProto.StatusResponse> ForwardDataBlock(DataNodeProto.DataBlock blockContainer, ServerCallContext context)
        {
            Console.WriteLine(blockContainer);

            // Get first node in list
            DataNodeProto.DataNode nodeToFoward = blockContainer.DataNodes[0];

            // Remove that node from list
            blockContainer.DataNodes.RemoveAt(0);

            // Write block to file system
            bool writeSuccess = BlockStorage.Instance.WriteBlock(Guid.Parse(blockContainer.BlockId.Value), blockContainer.Data.ToByteArray());

            // Once write is done, we can forward to next node
            // Need to connect to the nodes ip address
            Channel channel = new Channel("127.0.0.1:" + Program.Port, ChannelCredentials.Insecure);
            // TODO: Replace above call with actual IP
            //Channel channel = new Channel(nodeToFoward.Ipaddress + ":" + Program.Port, ChannelCredentials.Insecure);

            var client = new DataNodeProto.DataNodeProto.DataNodeProtoClient(channel);
            DataNodeProto.StatusResponse nodeResponse = client.ForwardDataBlock(blockContainer);

            bool forwardSuccess = nodeResponse.Type == DataNodeProto.StatusResponse.Types.StatusType.Success ? true : false;

            DataNodeProto.StatusResponse.Types.StatusType response;
            if (writeSuccess && forwardSuccess)
            {
                response = DataNodeProto.StatusResponse.Types.StatusType.Success;
            }
            else
            {
                response = DataNodeProto.StatusResponse.Types.StatusType.Fail;
            }

            return Task.FromResult(new DataNodeProto.StatusResponse { Type = response });
        }
    }
}
