using Grpc.Core;
using System;

namespace Client
{
  class FileReader
  {
        private static ClientProto.ClientProto.ClientProtoClient client;

        public FileReader(ClientProto.ClientProto.ClientProtoClient nameNodeClient)
        {
            client = nameNodeClient;
        }
        
        /// <summary>
        /// Creates the pipeline for streaming the block to DataNodes
        /// </summary>
        /// <param name="blockInfo">Info of the block. Contains blockid, blocksize, and ipaddresses for pipeline creation</param>
        /// <param name="client">Client connection with first DataNode in pipe</param>
        /// <returns>Success of pipeline creation</returns>

        private bool GetPipeLineReady(ClientProto.BlockInfo blockInfo, out ClientProto.ClientProto.ClientProtoClient client)
        {
            ClientProto.StatusResponse readyResponse = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            
            try
            {
                Channel channel = new Channel(blockInfo.IpAddress[0] + ":" + "50051", ChannelCredentials.Insecure);
                // TODO: Remove debugging code
                //Channel channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);
                //channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);

                blockInfo.IpAddress.RemoveAt(0);
                client = new ClientProto.ClientProto.ClientProtoClient(channel);

                readyResponse = client.GetReady(blockInfo);

                // Mainly for debugging
                if (readyResponse.Message != null || readyResponse.Message != "")
                {
                    Console.WriteLine(readyResponse.Message);
                }
                return true;
            }
            catch (RpcException e)
            {
                // Can't connect to first node -> Need to contact namenode or try other datanode
                Console.WriteLine("Get ready failed: " + e.Message);
                client = null;
                return false;
            }
        }
    
    
  
  }
}
