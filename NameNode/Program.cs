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
    class Program
    {
        //public static List<DataNode> nodeList = new List<DataNode>();
        public static Database Database = new Database();
        static void Main(string[] args)
        {

            Console.WriteLine("Initializing NameNode");
            // Use ec2 instance manager to get the private ip address of this name node
            EC2InstanceManager.InstanceManager instanceManager = EC2InstanceManager.InstanceManager.Instance;
            string ipAddress = instanceManager.GetPrivateIpAddress();

//#if !DEBUG
//            instanceManager.OpenFirewallPort(Constants.Port.ToString()); // Need to open port on windows firewalls
//#endif

#if DEBUG
            if (ipAddress == null)
            {
                ipAddress = "localhost";
            }
#endif

            Server server = new Server
            {
                Services = { DataNodeProto.DataNodeProto.BindService(new DataNodeHandler()),
                   ClientProto.ClientProto.BindService(new ClientHandler())},
                Ports = { new ServerPort(ipAddress, Constants.Port, ServerCredentials.Insecure) }
            };

            Console.WriteLine("Done initializing");

            server.Start();

            DataNodeManager nodeManager = DataNodeManager.Instance;
            //Check for dead datanodes
            Task nodeCheckTask = nodeManager.RunNodeCheck();

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
    }
}
