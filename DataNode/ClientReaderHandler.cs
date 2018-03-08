using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    class ClientHandler : ClientProto.ClientProto.ClientProtoBase
    {
        // step1: read file from disk;   pass in parameter should be UUID?
        private void ReadFileFromDisk(string filePath = @"C:\Users\Zach Madigan\Documents\Cloud Computing\CC-MAIN-20180116070444-20180116090444-00000.warc.gz")
        {
            try
            {
                FileInfo info = new FileInfo(filePath);
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 2097152))
                using (BufferedStream responseStream = new BufferedStream(fs, Constants.ChunkSize))
                {
                    ProcessStream(responseStream, info.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Reading from disk failed: " + e.Message);
            }
        }
        
        // step2: process stream
        private void ProcessStream(Stream stream, long contentLength)
        {
            // Read from response stream until entire file is read
            long totalBytesRead = 0;
            byte[] block = new byte[Constants.BlockSize];
            while (totalBytesRead < contentLength)
            {
                try
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    int bytesRead = 0;

                    // Read until block size or until all the file has been read
                    while (bytesRead < Constants.BlockSize && totalBytesRead != contentLength)
                    {
                        int length = Math.Min(block.Length, Constants.BlockSize - bytesRead);
                        int n = stream.Read(block, bytesRead, length);
                        bytesRead += n;
                        totalBytesRead += n;
                    }

                    watch.Stop();
                    Console.WriteLine("Total time to Read data from stream: " + watch.Elapsed);

                    ClientProto.BlockInfo blockInfo = new ClientProto.BlockInfo
                    {
                        BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() },
                        BlockSize = bytesRead
                    };

                    // Get DataNode locations to store block
                    blockInfo = client.QueryBlockDestination(blockInfo);

                    // Keep asking NameNode for new DataNodes if it fails
                    do
                    {
                        // Get DataNode locations to store block
                        blockInfo = client.QueryBlockDestination(blockInfo);
                    } while (!CreatePipelineAndWrite(blockInfo, block));
                }
                catch (RpcException e)
                {
                    // Stop reading
                    totalBytesRead = contentLength;
                    Console.WriteLine("Query for block destination failed: " + e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        
        // step3: transmit data
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
        
        
        // Server side handler of ReadBlock  RPC
        public override async Task ReadBlock(ClientProto.BlockInfo blockInfo, Grpc.Core.IServerStreamWriter<> responseStream, Grpc.Core.ServerCallContext context)
        {
            
        }
    }
}
