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
        public static Database Database;
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing NameNode");
            Database = new Database();
            DataNodeManager nodeManager = DataNodeManager.Instance;
            // Use ec2 instance manager to get the private ip address of this name node
            EC2InstanceManager.InstanceManager instanceManager = EC2InstanceManager.InstanceManager.Instance;
            string ipAddress = instanceManager.GetPrivateIpAddress();

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

            //Check for dead datanodes
            Task nodeCheckTask = nodeManager.RunNodeCheck();

            Console.WriteLine("Greeter server listening on ip address " + ipAddress + ", port " + Constants.Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
