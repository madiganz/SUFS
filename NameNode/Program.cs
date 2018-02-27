using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using NameNode.FileSystem;
using System.IO;
using YamlDotNet.Serialization.NamingConventions;
using ClientProto;

namespace NameNode
{
    class ClientImpl : ClientProto.ClientProto.ClientProtoBase
    {

        public override Task<StatusResponse> DeleteDirectory(ClientProto.Path path, ServerCallContext context)
        {
            if (Program.Database.DeleteDirectory(path.Fullpath))
                return Task.FromResult(new StatusResponse { Type = StatusResponse.Types.StatusType.Success });
            else
                return Task.FromResult(new StatusResponse { Type = StatusResponse.Types.StatusType.Fail });
        }

        //?
        public override Task<StatusResponse> AddDirectory(ClientProto.Path path, ServerCallContext context)
        {
            Program.Database.CreateDirectory(path.Fullpath);
            return Task.FromResult(new StatusResponse { Type = StatusResponse.Types.StatusType.Success });
        }


        public override Task<ListOfNodes> ListNodes(ClientProto.Path path, ServerCallContext context)
        {
            Program.Database.ListDataNodesStoringReplicas();
        }

        //*
        public override Task<StatusResponse> DeleteFile(ClientProto.Path path, ServerCallContext context)
        {
            Program.Database.DeleteFile(path.Fullpath);
            return Task.FromResult(new StatusResponse { Type = StatusResponse.Types.StatusType.Success });
        }

        public override Task<ListOfContents> ListContents(ClientProto.Path path, ServerCallContext context)
        {
            List<String> filename = new List<String>();
            filename = Program.Database.ListDirectoryContents(path.Fullpath);
            ListOfContents contents = new ListOfContents
            {
                fileName = {filename}
            };
            return Task.FromResult(contents);
        }

        public override Task<StatusResponse> CreateFile(ClientProto.Path path, ServerCallContext context)
        {
            var response = new StatusResponse { Type = StatusResponse.Types.StatusType.Ok };
            if (Program.Database.FileExists(path.FullPath))
            {
                response = new StatusResponse { Type = StatusResponse.Types.StatusType.FileExists };
            }
            return Task.FromResult(response);
        }

        //public override Task<ClientProto.BlockList> QueryBlockDestination(ClientProto.BlockList blockList, ServerCallContext context)
        public override Task<ClientProto.BlockInfo> QueryBlockDestination(ClientProto.BlockInfo blockInfo, ServerCallContext context)
        {
            //List<ClientProto.BlockInfo> blockInfoList = new List<ClientProto.BlockInfo>();
            //foreach (var blockInfo in blockList.BlockInfo)
            //{
                // TODO: replace with real query to get ipaddresses
                List<string> ipAddresses = new List<string>()
                {
                    //"test",
                    //"test",
                    //"test"
                };

                // Hopefully I can find a better way to do this
                ClientProto.BlockInfo blckInfo = new ClientProto.BlockInfo
                {
                    BlockId = blockInfo.BlockId,
                    BlockSize = blockInfo.BlockSize,
                    IpAddress = { ipAddresses }
                };
                //blockInfoList.Add(blckInfo);
            //}

            //return Task.FromResult(new ClientProto.BlockList { BlockInfo = { blockInfoList } });
            return Task.FromResult(blckInfo);
        }
    }

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
            int index = Program.nodeList.FindIndex(node => node.ipAddress == request.NodeInfo.DataNode.IpAddress);

            // Not found -> add to list
            if (index < 0)
            {
                DataNode node = new DataNode(request.NodeInfo.DataNode.IpAddress, request.NodeInfo.DiskSpace, DateTime.UtcNow);
                Program.nodeList.Add(node);
            }
            else // Found, update lastHeartBeat timestamp
            {
                Program.nodeList[index].diskSpace = request.NodeInfo.DiskSpace;
                Program.nodeList[index].lastHeartBeat = DateTime.UtcNow;
            }

            // Create fake list of blocks to invalidate
            List<DataNodeProto.UUID> blocks = new List<DataNodeProto.UUID>();
            for (var i = 0; i < 11; i++)
            {
                blocks.Add(new DataNodeProto.UUID { Value = Guid.NewGuid().ToString() });
            }

            // Create command
            DataNodeProto.DataNodeCommands commands = new DataNodeProto.DataNodeCommands
            {
                Command = new DataNodeProto.BlockCommand
                {
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
        public static List<DataNode> nodeList = new List<DataNode>();
        public static Database Database = new Database();
        static void Main(string[] args)
        {

            Console.WriteLine("Initializing NameNode");
            // Use ec2 instance manager to get the private ip address of this name node
            EC2InstanceManager.InstanceManager instanceManager = EC2InstanceManager.InstanceManager.Instance;
            string ipAddress = instanceManager.GetPrivateIpAddress();

#if !DEBUG
            instanceManager.OpenFirewallPort(Constants.Port.ToString()); // Need to open port on windows firewalls
#endif

#if DEBUG
            if (ipAddress == null)
            {
                ipAddress = "localhost";
            }
#endif

            Server server = new Server
            {
                Services = { DataNodeProto.DataNodeProto.BindService(new NameNodeImpl()),
                ClientProto.ClientProto.BindService(new ClientImpl())},
                Ports = { new ServerPort(ipAddress, Constants.Port, ServerCredentials.Insecure) }
            };

            Console.WriteLine("Done initializing");

            server.Start();

            //Check for dead datanodes
            Task nodeCheckTask = RunNodeCheck();

            Console.WriteLine("Greeter server listening on ip address " + ipAddress + ", port " + Constants.Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            //TestSavingFileSystem();
            //TestLoadingFileSystem();

            server.ShutdownAsync().Wait();
        }

        private static void TestSavingFileSystem()
        {
            var Root = new Folder("root", null);
            Root.subfolders.Add("user", new Folder("user", "root"));
            Folder currentLocation = Root.subfolders["user"];
            currentLocation.subfolders.Add("other folder", new Folder("other folder", currentLocation.path));
            currentLocation.files.Add("File",  new FileSystem.File("File", currentLocation.path));
            FileSystem.File selectedFile = currentLocation.files["File"];

            var guid = Guid.NewGuid();
            selectedFile.fileSize = 88;
            List<string> ips = new List<string>();
            selectedFile.data.Add(guid);

            guid = Guid.NewGuid();
            selectedFile.data.Add(guid);

            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Root);
            System.IO.File.WriteAllText(@"C:/data/testDirecotry.yml", yaml);
            Console.WriteLine(yaml);
        }

        private static void TestLoadingFileSystem()
        {
            string yaml = System.IO.File.ReadAllText(@"C:/data/testDirecotry.yml");
            var input = new StringReader(yaml);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var Root = deserializer.Deserialize<Folder>(input);

            Console.WriteLine(Root.name);

            Folder currentLocation = Root.subfolders["user"];
            Console.WriteLine(currentLocation.name);
            Folder subLocation = currentLocation.subfolders["other folder"];
            Console.WriteLine(subLocation.name);
            FileSystem.File selectedFile = currentLocation.files["File"];
            Console.WriteLine(selectedFile.name);

            Console.WriteLine(selectedFile.data.Count());

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
