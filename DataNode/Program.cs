using Grpc.Core;
using System;
using System.Threading;
using Amazon.EC2;



namespace DataNode
{
    class Program
    {
        public static string ipAddress;
        public const int Port = 50052;
        public const int Port2 = 50051; // TODO: Ports should be fixed better
        public static Guid mNodeID = Guid.NewGuid();
        static void Main(string[] args)
        {
            //GetEC2IpAddress();

            Server server = new Server
            {
                Services = { DataNodeProto.DataNodeProto.BindService(new DataNodeImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Channel channel = new Channel("127.0.0.1:" + Port2, ChannelCredentials.Insecure);
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

        /// <summary>
        /// Gets the the EC2 instances ip address so it can correctly bind to the host and port
        /// </summary>
        public static void GetEC2IpAddress()
        {
            try
            {
                Amazon.Util.EC2InstanceMetadata.InstanceId.ToString();
                AmazonEC2Client myInstance = new AmazonEC2Client();
                Amazon.EC2.Model.DescribeInstancesRequest request = new Amazon.EC2.Model.DescribeInstancesRequest();
                request.InstanceIds.Add(Amazon.Util.EC2InstanceMetadata.InstanceId);
                Amazon.EC2.Model.DescribeInstancesResponse response = myInstance.DescribeInstances(request);
                bool success = response.ResponseMetadata.Metadata.TryGetValue("public-ipv4", out ipAddress);

                // Hopefully only for debugging purposes!!!!
                if (!success)
                {
                    ipAddress = "localhost";
                }
            }
            catch
            {
                ipAddress = "localhost";
            }
        }
    }
}
