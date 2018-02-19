using Grpc.Core;
using System;
using System.Threading;



namespace DataNode
{
    class Program
    {
        public static string ipAddress;
        public const int Port = 50051;
        public static Guid mNodeID = Guid.NewGuid();
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing DataNode");
            string nameNodeIp = args[0];

            // Use ec2 instance manager to get the private ip address of this data node
            EC2InstanceManager.InstanceManager instanceManager = EC2InstanceManager.InstanceManager.Instance;
            ipAddress = instanceManager.GetPrivateIpAddress();
            instanceManager.OpenFirewallPort("50051"); // Need to open port on windows firewalls

#if DEBUG
            if (ipAddress == null)
            {
                ipAddress = "localhost";
            }
#endif

            Server server = new Server
            {
                Services = { DataNodeProto.DataNodeProto.BindService(new DataNodeImpl()) },
                Ports = { new ServerPort(ipAddress, Port, ServerCredentials.Insecure) }
            };

            Console.WriteLine("Done initializing");

            server.Start();

            Console.WriteLine("Trying to connect to: " + nameNodeIp);
            Channel channel = new Channel(nameNodeIp + ":" + Port, ChannelCredentials.Insecure);
            var client = new DataNodeProto.DataNodeProto.DataNodeProtoClient(channel);

            //Initialize blockstorage
            BlockStorage mBlockStorage = BlockStorage.Instance;
            Thread heartBeatThread = new Thread(new ParameterizedThreadStart(HeartBeat.SendHeartBeat));
            Thread blockReportThread = new Thread(new ParameterizedThreadStart(BlockReport.SendBlockReport));
            heartBeatThread.Start(client);
            blockReportThread.Start(client);
            while (true)
            {
            }
        }
    }
}
