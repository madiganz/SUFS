using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NameNode
{
    class NameNodeImpl : DataNodeProto.DataNodeProto.DataNodeProtoBase
    {
        // Server side handler of the SendBlockReportRequest RPC
        public override Task<DataNodeProto.StatusResponse> SendBlockReport(DataNodeProto.BlockReportRequest request, ServerCallContext context)
        {
            Console.WriteLine(request);
            return Task.FromResult(new DataNodeProto.StatusResponse { Type = DataNodeProto.StatusResponse.Types.StatusType.Success });
        }

        // Server side handler of the SendHeartBeat RPC
        public override Task<DataNodeProto.HeartBeatResponse> SendHeartBeat(DataNodeProto.HeartBeatRequest request, ServerCallContext context)
        {
            Console.WriteLine(request);

            // Update list of datanodes
            int index = Program.nodeList.FindIndex(node => node.nodeId == Guid.Parse(request.NodeInfo.DataNode.Id.Value));

            // Not found -> add to list
            if(index < 0)
            {
                DataNode node = new DataNode(request.NodeInfo.DataNode.Id.Value, request.NodeInfo.DataNode.IpAddress, DateTime.UtcNow);
                Program.nodeList.Add(node);
            }
            else // Found, update lastHeartBeat timestamp
            {
                Program.nodeList[index].lastHeartBeat = DateTime.UtcNow;
            }
            
            // Create fake list of blocks to invalidate
            List<DataNodeProto.UUID> blocks = new List<DataNodeProto.UUID>();
            for(var i = 0; i < 11; i++)
            {
                blocks.Add(new DataNodeProto.UUID { Value = Guid.NewGuid().ToString() });
            }

            // Create command
            DataNodeProto.DataNodeCommands commands = new DataNodeProto.DataNodeCommands
            {
                Command = new DataNodeProto.BlockCommand {
                    Action = DataNodeProto.BlockCommand.Types.Action.Delete,
                    BlockList = new DataNodeProto.BlockList
                    {
                        BlockId = { blocks }
                    }
                }

            };

            DataNodeProto.HeartBeatResponse response = new DataNodeProto.HeartBeatResponse
            {
                Commands = { commands }
            };

            return Task.FromResult(response);
        }
    }

    class Program
    {
        const int Port = 50051;
        public static List<DataNode> nodeList = new List<DataNode>();
        static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { DataNodeProto.DataNodeProto.BindService(new NameNodeImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            //Check for dead datanodes
            Task nodeCheckTask = RunNodeCheck();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }

        public static async Task RunNodeCheck(CancellationToken token = default(CancellationToken))
        {
            while (!token.IsCancellationRequested)
            {
                CheckDeadNodes();
                try
                {
                    await Task.Delay(60000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private static void CheckDeadNodes()
        {
            Console.WriteLine("Checking dead nodes");
            foreach (var node in nodeList)
            {
                TimeSpan span = DateTime.UtcNow.Subtract(node.lastHeartBeat);
                // Too much time has passed
                if (span.Minutes >= 10)
                {
                    nodeList.Remove(node);
                }

            }
        }
    }
}
