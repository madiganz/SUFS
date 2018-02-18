using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

using NameNode.FileSystem;
using System.IO;
using YamlDotNet.Serialization.NamingConventions;

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

            TestSavingFileSystem();
            TestLoadingFileSystem();

            server.ShutdownAsync().Wait();
        }

        private static void TestSavingFileSystem()
        {
            var Root = new Folder("root", null);
            Root.subfolders.Add("user", new Folder("user", Root));
            Folder currentLocation = Root.subfolders["user"];
            currentLocation.subfolders.Add("other folder", new Folder("other folder", currentLocation));
            currentLocation.files.Add("File", new FileSystem.File("File"));
            FileSystem.File selectedFile = currentLocation.files["File"];

            var guid = Guid.NewGuid();
            selectedFile.fileSize = 88;
            List<string> ips = new List<string>();
            selectedFile.data.Add(guid);

            guid = Guid.NewGuid();
            selectedFile.data.Add(guid);

            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Root);
            System.IO.File.WriteAllText(@"/Users/BryanHerr/Documents/College/CPSC4910-02/SUFS/testDirecotry.yml",yaml);
            Console.WriteLine(yaml);
        }

        private static void TestLoadingFileSystem()
        {
            string yaml = System.IO.File.ReadAllText(@"/Users/BryanHerr/Documents/College/CPSC4910-02/SUFS/testDirecotry.yml");
            var input = new StringReader(yaml);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var Root = deserializer.Deserialize<Folder>(input);

            Console.WriteLine(Root.name);

            Folder currentLocation = Root.subfolders["user"];
            Console.WriteLine(currentLocation.name);
            currentLocation = currentLocation.subfolders["other folder"];
            Console.WriteLine(currentLocation.name);
            currentLocation = currentLocation.parent;
            FileSystem.File selectedFile = currentLocation.files["File"];
            Console.WriteLine(selectedFile.name);

            Console.WriteLine(selectedFile.data.Count());

        }

    }
}
