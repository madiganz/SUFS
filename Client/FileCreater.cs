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

namespace Client
{
    /// <summary>
    /// Class to manage creating a file in SUFS
    /// </summary>
    class FileCreater
    {
        private static string filePath;
        private static string localPath;
        private static long TotalBytesWrittenFromFile;
        private static ClientProto.ClientProto.ClientProtoClient client;
        public FileCreater(ClientProto.ClientProto.ClientProtoClient nameNodeClient)
        {
            client = nameNodeClient;
            TotalBytesWrittenFromFile = 0;
        }

        /// <summary>
        /// Creates a file in SUFS
        /// </summary>
        /// <param name="location">Location of file. Defaults to local disk</param>
        public void CreateFile(string path, string lPath, string location = "local")
        {
            filePath = path;
            localPath = lPath;
            var response = client.CreateFile(new ClientProto.Path { FullPath = filePath });
           
            if (response.Type == ClientProto.StatusResponse.Types.StatusType.Ok)
            {
                if (location.ToLower() == "s3")
                {
                    ReadFileFromS3(lPath);
                }
                else
                {
                    ReadFileFromDisk(lPath);
                }
                Console.WriteLine();
                Console.WriteLine("Done writing file");
            }
            else
            {
                Console.WriteLine("File already exists!");
            }
        }

        /// <summary>
        /// Writes a file to SUFS using a file from S3
        /// </summary>
        private async void ReadFileFromS3(string key = "CC-MAIN-20180116070444-20180116090444-00000.warc.gz", string bucketName = "wordcount-madiganz")
        {
            IAmazonS3 s3Cient;
            using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
            {
                try
                {
                    // Request object
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    };

                    using (GetObjectResponse response = await s3Cient.GetObjectAsync(request))
                    using (BufferedStream responseStream = new BufferedStream(response.ResponseStream, Constants.ChunkSize))
                    {
                        ProcessStream(responseStream, response.ContentLength);
                    };
                }
                catch (Exception e)
                {
                    client.DeleteFile(new ClientProto.Path { FullPath = filePath });
                    Console.WriteLine("Amazon S3 failed: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Writes a file to SUFS using a file from local disk
        /// </summary>
        private void ReadFileFromDisk(string filePath = "./CC-MAIN-20180116070444-20180116090444-00000.warc.gz")
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
                client.DeleteFile(new ClientProto.Path { FullPath = filePath });
                Console.WriteLine("Reading from disk failed: " + e.Message);
            }
        }

        /// <summary>
        /// Processes stream in order to read the file
        /// </summary>
        /// <param name="stream">Stream of the file</param>
        /// <param name="contentLength">Length of the file</param>
        private void ProcessStream(Stream stream, long contentLength)
        {
            // Read from response stream until entire file is read
            long totalBytesRead = 0;
            byte[] block = new byte[Constants.BlockSize];
            while (totalBytesRead < contentLength)
            {
                try
                {
                    int bytesRead = 0;

                    // Read until block size or until all the file has been read
                    while (bytesRead < Constants.BlockSize && totalBytesRead != contentLength)
                    {
                        int length = Math.Min(block.Length, Constants.BlockSize - bytesRead);
                        int n = stream.Read(block, bytesRead, length);
                        bytesRead += n;
                        totalBytesRead += n;
                    }

                    ClientProto.BlockInfo blockInfo = new ClientProto.BlockInfo
                    {
                        BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() },
                        FullPath = filePath,
                        BlockSize = bytesRead
                    };

                    bool writeSuccess = false;
                    while (!writeSuccess)
                    {
                        // Keep asking NameNode for new DataNodes if all fail
                        // Get DataNode locations to store block
                        blockInfo = client.QueryBlockDestination(blockInfo);
                        for (int i = 0; i < blockInfo.IpAddress.Count(); i++)
                        {
                            // Need to reassign because classes are passed by reference and we are removing nodes
                            ClientProto.BlockInfo info = blockInfo;
                            if (CreatePipelineAndWrite(info, block, contentLength))
                            {
                                writeSuccess = true;
                                break;
                            }
                            else
                            {
                                // Try a different datanode first.
                                // This has a chance because this flow only fails if the first datanode fails to connect or somehow they all fail.
                                // Not sure if this is the best way to handle it, but it works
                                blockInfo.IpAddress.Clear();
                                blockInfo.IpAddress.AddRange(blockInfo.IpAddress.Shift());
                            }
                        }
                    }
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

        /// <summary>
        /// Creates the pipeline and writes to datanode
        /// </summary>
        /// <param name="blockInfo">Info of the block. Contains blockid, blocksize, and ipaddresses for pipeline creation</param>
        /// <param name="block">data of block</param>
        /// <returns>Returns true if streaming of data through newly created pipeline was successful</returns>
        private bool CreatePipelineAndWrite(ClientProto.BlockInfo blockInfo, byte[] block, long contentLength)
        {
            // Create pipeline from list
            if (GetPipeLineReady(blockInfo, out ClientProto.ClientProto.ClientProtoClient writeClient))
            {
                if (client != null)
                {
                    try
                    {
                        // Send block through pipeline
                        WriteBlock(writeClient, blockInfo, block, contentLength).Wait();
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
            }
            return false;
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

                blockInfo.IpAddress.RemoveAt(0);
                client = new ClientProto.ClientProto.ClientProtoClient(channel);

                readyResponse = client.GetReady(blockInfo);

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

        /// <summary>
        /// Writes the block to the first DataNode
        /// </summary>
        /// <param name="client">Client Connection to the first DataNode</param>
        /// <param name="blockInfo">Info of the block. Contains blockid, blocksize, and ipaddresses for pipeline creation</param>
        /// <param name="block">Data of block</param>
        /// <returns>Task of the write or Exception on fail</returns>
        private async Task WriteBlock(ClientProto.ClientProto.ClientProtoClient client, ClientProto.BlockInfo blockInfo, byte[] block, long contentLength)
        {
            Metadata metaData = new Metadata {
                new Metadata.Entry("blockid", blockInfo.BlockId.Value),
                new Metadata.Entry("blocksize", blockInfo.BlockSize.ToString())
            };

            int totalBytesRead = 0;

            using (var call = client.WriteBlock(metaData))
            {
                bool dataNodeFailed = false;

                while (totalBytesRead < blockInfo.BlockSize)
                {
                    int length = Math.Min(Constants.ChunkSize, blockInfo.BlockSize - totalBytesRead);

                    // Make sure correct length of data is sent
                    byte[] chunk = new byte[length];

                    Buffer.BlockCopy(block, totalBytesRead, chunk, 0, length);

                    totalBytesRead += length;
                    TotalBytesWrittenFromFile += length;

                    try
                    {
                        await call.RequestStream.WriteAsync(new ClientProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(chunk) });
                        // Casts are very necessary here!
                        Console.Write("\rWriting File {0}", (((double)TotalBytesWrittenFromFile / (double)contentLength)).ToString("0.00%"));
                    }
                    catch (RpcException e)
                    {
                        dataNodeFailed = true;
                        totalBytesRead = blockInfo.BlockSize; // Stop reading
                        throw new Exception("Writing block failed", e);
                    }
                }

                ClientProto.StatusResponse resp = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                if (!dataNodeFailed)
                {
                    await call.RequestStream.CompleteAsync();
                    resp = await call.ResponseAsync;
                }
                if (resp.Type == ClientProto.StatusResponse.Types.StatusType.Fail)
                {
                    throw new Exception("Writing block failed");
                }
            }
        }
    }
}