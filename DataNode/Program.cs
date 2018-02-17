using Grpc.Core;
using System;
using System.Threading;

namespace DataNode
{
    class Program
    {
        public static Guid mNodeID = Guid.NewGuid();
        static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            var client = new DataNodeProto.DataNodeService.DataNodeServiceClient(channel);

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
