using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataNode
{
    class Program
    {
        public static string ipAddress;
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing DataNode");
            string nameNodeIp = args[0];

            int port = Convert.ToInt32(args[1]);

            // Use ec2 instance manager to get the private ip address of this data node
            EC2InstanceManager.InstanceManager instanceManager = EC2InstanceManager.InstanceManager.Instance;
            ipAddress = instanceManager.GetPrivateIpAddress();

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
                //Ports = { new ServerPort(ipAddress, Constants.Port, ServerCredentials.Insecure) }
                Ports = { new ServerPort(ipAddress, port, ServerCredentials.Insecure) }
            };

            Console.WriteLine("Done initializing");

            server.Start();

            Console.WriteLine("Trying to connect to: " + nameNodeIp);
            Channel channel = new Channel(nameNodeIp + ":" + Constants.Port, ChannelCredentials.Insecure);
            ConnectionManager.Instance.NameNodeConnection = channel;
            var client = new DataNodeProto.DataNodeProto.DataNodeProtoClient(channel);

            // Initialize blockstorage
            BlockStorage mBlockStorage = BlockStorage.Instance;

            Task heartBeatTask = HeartBeat.SendHeartBeat(client);
            Task blockReportTask = BlockReport.SendBlockReport(client);
            while (true)
            {

            }
        }
    }
}
