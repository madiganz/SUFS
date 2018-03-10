using Amazon.S3;
using Amazon.S3.Model;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Client
{
  class FileReader
  {
        private const int BufferSize = 1;

        private static ClientProto.ClientProto.ClientProtoClient client;

        public FileReader(ClientProto.ClientProto.ClientProtoClient nameNodeClient)
        {
            client = nameNodeClient;
        }
        public void ReadFile(String readPath, String writePath)
        {
            var buffer = new byte[BufferSize];
            var writeFileStream = new FileStream(writePath, FileMode.CreateNew, FileAccess.ReadWrite);
            List<ClientProto.BlockInfo> nodeList = GetList(readPath);
            //Read from each DataNode
            nodeList.ForEach(ReadFromNode);
            
            
        }

        //Ask NameNode for list
        private List<ClientProto.BlockInfo> GetList(String path)
        {
            List<ClientProto.BlockInfo> blockInfo = client.ReadFile(new ClientProto.Path { FullPath = path }).BlockInfo.ToList();
            return blockInfo;
        }

        //Read file from DataNode
        private void ReadFromNode(ClientProto.BlockInfo blockInfo)
        {
            while (!blockInfo.IpAddress.Any())
            {
                if (GetReady(blockInfo))
                {
                    ReadBlock(blockInfo.BlockId.Value);
                    break;
                }
                else
                {
                    blockInfo.IpAddress.RemoveAt(0);
                }
            }

        }

        private bool GetReady(ClientProto.BlockInfo blockInfo)
        {
            ClientProto.StatusResponse readyResponse = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            try
            {
                Channel channel = new Channel(blockInfo.IpAddress[0] + ":" + "50051", ChannelCredentials.Insecure);
               
                //blockInfo.IpAddress.RemoveAt(0);
                client = new ClientProto.ClientProto.ClientProtoClient(channel);

                readyResponse = client.GetReady(blockInfo);

                return true;
            }
            catch (RpcException e)
            {
                Console.WriteLine("Get ready failed: " + e.Message);
                client = null;
                return false;
            }
        }

        private void ReadBlock(String id)
        {
            try
            {
                ClientProto.UUID blockId = new ClientProto.UUID { Value = id };

                byte[] byteData = client.ReadBlock(blockId).ToByteArray();
                //write here?
            }
            catch (RpcException e)
            {
                throw new Exception("RPC failed " , e);
            }
        }

        private void WriteFile()
        {

        }
    
    
  
  }
}
