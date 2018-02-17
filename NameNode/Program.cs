using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Console.WriteLine(request.HeartBeatString);

            // Create fake list of blocks to invalidate
            List<DataNodeProto.UUID> blocks = new List<DataNodeProto.UUID>();
            for(var i = 0; i < 11; i++)
            {
                blocks.Add(new DataNodeProto.UUID { Value = Guid.NewGuid().ToString() });
            }

            // Create command
            DataNodeProto.DataNodeCommands commands = new DataNodeProto.DataNodeCommands
            {
                CmdType = DataNodeProto.DataNodeCommands.Types.Type.BlockCommand,
                BlkCmd = new DataNodeProto.BlockCommandProto {
                    Action = DataNodeProto.BlockCommandProto.Types.Action.Invalidate,
                    BlockList = new DataNodeProto.BlockList
                    {
                        BlockId = { blocks }
                    }
                }

            };

            DataNodeProto.HeartBeatResponse response = new DataNodeProto.HeartBeatResponse
            {
                Cmds = { commands }
            };

            return Task.FromResult(response);
        }
    }

    class Program
    {
        const int Port = 50051;

        static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { DataNodeProto.DataNodeProto.BindService(new NameNodeImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
