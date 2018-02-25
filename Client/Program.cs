using Amazon.S3;
using Amazon.S3.Model;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        public static void Main(string[] args)
        {
            //// Assume passed in parameters:
            //// NameNodeIP:Port => args[0],
            Channel channel = new Channel(args[0], ChannelCredentials.Insecure);

            // Use "write" to write blocks to nodes. WriteBlock function takes some manual config
            string task = args[1];
            //Channel channel = new Channel(IpAddress + ":50051", ChannelCredentials.Insecure);
            //Channel channel = new Channel("127.0.0.1" + ":50051", ChannelCredentials.Insecure);

            var client = new ClientProto.ClientProto.ClientProtoClient(channel);

            //var reply = client.DeleteDirectory(new ClientProto.Path { Fullpath = "Path" });

            if (task == "create")
            {
                CreateFile(client);
            }

            //if (task == "write")
            //{
            //    //ClientProto.BlockMessage blockMessage = client.CreateFile(new ClientProto.NewFile { Fullpath = "path" });
            //    //foreach(var blockInfo in blockMessage.BlockInfo)
            //    //{
            //    //    WriteBlock(client, blockInfo).Wait();
            //    //}
            //    List<string> addresses = new List<string>
            //    {
            //        "172.31.40.133"
            //    };
            //    ClientProto.StatusResponse readyResponse = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            //    Guid blockId = Guid.NewGuid();
            //    try
            //    {
            //        // readyResponse should always be Ready if it was able to connect
            //        readyResponse = client.GetReady(new ClientProto.BlockInfo
            //        {
            //            BlockId = new ClientProto.UUID { Value = blockId.ToString() },
            //            IpAddress = { addresses }
            //        });

            //        // Mainly for debugging
            //        if (readyResponse.Message != null)
            //        {
            //            Console.WriteLine(readyResponse.Message);
            //        }

            //        WriteBlock(client, blockId).Wait();
            //    }
            //    catch (RpcException e)
            //    {
            //        // Can't connect to first node -> Need to contact namenode or try other datanode
            //        Console.WriteLine("Get ready failed: " + e.Message);
            //    }
            //}

            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static void CreateFile(ClientProto.ClientProto.ClientProtoClient client)
        {
            var response = client.CreateFile(new ClientProto.Path { FullPath = @"C:\test\example.warc" });
            Console.WriteLine(response);

            if (response.Type == ClientProto.StatusResponse.Types.StatusType.Ok)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                ReadFileFromS3(client);
                watch.Stop();
                Console.WriteLine("Total to create file: " + watch.Elapsed);
            }
            else
            {
                Console.WriteLine("File already exists!");
            }
        }

        //public static void ReadFileFromS3(ClientProto.ClientProto.ClientProtoClient client)
        //{
        //    IAmazonS3 s3Cient;
        //    using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
        //    {
        //        GetObjectRequest request = new GetObjectRequest
        //        {
        //            BucketName = "wordcount-madiganz",
        //            Key = "CC-MAIN-20180116070444-20180116090444-00000.warc"
        //            //ByteRange = new ByteRange(0, 134217727) // 128 MB,
        //        };

        //        //byte[] block = new byte[2097152];
        //        byte[] block = new byte[134217727];

        //        long totalBytesRead = 0;

        //        using (GetObjectResponse response = s3Cient.GetObject(request))
        //        using (Stream responseStream = response.ResponseStream)
        //        {
        //            long totalBytesToRead = response.ContentLength;
        //            while (totalBytesRead < response.ContentLength)
        //            {
        //                int numBytesRead = 0;
        //                if (totalBytesToRead < 134217727)
        //                {
        //                    int numBytesToRead = (int)totalBytesToRead;
        //                    do
        //                    {
        //                        int n = responseStream.Read(block, numBytesRead, numBytesToRead);

        //                        numBytesRead += n;
        //                        numBytesToRead -= n;
        //                        totalBytesRead += n;
        //                    } while (numBytesToRead > 0);
        //                }
        //                else
        //                {
        //                    int numBytesToRead = 134217727;
        //                    do
        //                    {
        //                        int n = responseStream.Read(block, numBytesRead, numBytesToRead);
        //                        numBytesRead += n;
        //                        numBytesToRead -= n;
        //                        totalBytesRead += n;
        //                    } while (numBytesToRead > 0);
        //                }

        //                try
        //                {
        //                    ClientProto.BlockInfo blockInfo = client.QueryBlockDestination(new ClientProto.BlockInfo { BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() }, BlockSize = numBytesRead });
        //                    Console.WriteLine(blockInfo);
        //                    if (GetPipeLineReady(blockInfo, out ClientProto.ClientProto.ClientProtoClient writeClient))
        //                    {
        //                        if (client != null)
        //                        {
        //                            WriteBlock(writeClient, blockInfo, block).Wait();
        //                        }
        //                    }
        //                }
        //                catch (RpcException e)
        //                {
        //                    totalBytesRead = response.ContentLength; // Stop reading
        //                    Console.WriteLine("Query for block destination failed: " + e.Message);
        //                }
        //            }

        //            responseStream.Flush();
        //            responseStream.Dispose();
        //            response.Dispose();
        //        };
        //    }
        //}

        //public static void ReadFileFromS3(ClientProto.ClientProto.ClientProtoClient client)
        //{
        //    IAmazonS3 s3Cient;
        //    using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
        //    {
        //        try
        //        {
        //            long totalBytesRead = 0;
        //            //int blockSize = 134217728; // 128MB
        //            //while (blockSize == 134217728)
        //            //{
        //            // Get everything
        //            GetObjectRequest request = new GetObjectRequest
        //            {
        //                BucketName = "wordcount-madiganz",
        //                Key = "CC-MAIN-20180116070444-20180116090444-00000.warc"
        //                //ByteRange = new ByteRange(totalBytesRead, totalBytesRead + 134217727) // 128 MB,
        //            };

        //            //byte[] block = new byte[2097152];
        //            byte[] block = new byte[134217728];

        //            using (GetObjectResponse response = s3Cient.GetObject(request))
        //            using (BufferedStream responseStream = new BufferedStream(response.ResponseStream, 2097152))
        //            //using (StreamReader responseStream = new StreamReader(bs))
        //            //using (Stream responseStream = response.ResponseStream)
        //            {
        //                while (totalBytesRead < response.ContentLength)
        //                {
        //                    try
        //                    {
        //                        Stopwatch watch = new Stopwatch();
        //                        watch.Start();
        //                        int bytesRead = 0;
        //                        int offset = (int)totalBytesRead;
        //                        while (bytesRead < 134217728)
        //                        {
        //                            int length = Math.Min(block.Length, 134217728 - bytesRead);
        //                            //bytesRead += responseStream.Read(block, offset + bytesRead, length);
        //                            bytesRead += responseStream.Read(block, bytesRead, length);
        //                        }
        //                        watch.Stop();
        //                        Console.WriteLine("Total time to Read data from stream: " + watch.Elapsed);
        //                        totalBytesRead += bytesRead;

        //                        Task.Run(() => StartWritingData(client, block, bytesRead));

        //                        //ClientProto.BlockInfo blockInfo = client.QueryBlockDestination(new ClientProto.BlockInfo { BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() }, BlockSize = bytesRead });

        //                        //if (GetPipeLineReady(blockInfo, out ClientProto.ClientProto.ClientProtoClient writeClient))
        //                        //{
        //                        //    if (client != null)
        //                        //    {
        //                        //        WriteBlock(writeClient, blockInfo, block).Wait();
        //                        //    }
        //                        //}
        //                    }
        //                    catch (RpcException e)
        //                    {
        //                        totalBytesRead = response.ContentLength; // Stop reading
        //                        Console.WriteLine("Query for block destination failed: " + e.Message);
        //                    }
        //                }

        //                responseStream.Flush();
        //            };
        //            //}
        //        }
        //        catch (AmazonS3Exception e)
        //        {
        //            Console.WriteLine("Exception message: {0}", e.Message);
        //        }
        //    }
        //}

        //public static void ReadFileFromS3(ClientProto.ClientProto.ClientProtoClient client)
        //{
        //    IAmazonS3 s3Cient;
        //    using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
        //    {
        //        try
        //        {
        //            long totalBytesRead = 0;
        //            //int blockSize = 134217728; // 128MB
        //            //while (blockSize == 134217728)
        //            //{
        //            // Get everything
        //            GetObjectRequest request = new GetObjectRequest
        //            {
        //                BucketName = "wordcount-madiganz",
        //                Key = "CC-MAIN-20180116070444-20180116090444-00000.warc"
        //                //ByteRange = new ByteRange(totalBytesRead, totalBytesRead + 134217727) // 128 MB,
        //            };

        //            //byte[] block = new byte[2097152];
        //            //byte[] block = new byte[134217728];

        //            using (GetObjectResponse response = s3Cient.GetObject(request))
        //            using (BufferedStream responseStream = new BufferedStream(response.ResponseStream, 2097152))

        //            //using (Stream responseStream = response.ResponseStream)
        //            {
        //                // Divide response into blocks
        //                List<ClientProto.BlockInfo> blockInfoList = new List<ClientProto.BlockInfo>();
        //                for (long i = 0; i < response.ContentLength; i += 134217728)
        //                {
        //                    int blockSize = 0;
        //                    blockSize = ((i + 134217728) < response.ContentLength) ? 134217728 : (int)(response.ContentLength - i);
        //                    ClientProto.BlockInfo blockInfo = new ClientProto.BlockInfo
        //                    {
        //                        BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() },
        //                        BlockSize = blockSize
        //                    };

        //                    blockInfoList.Add(blockInfo);
        //                }

        //                ClientProto.BlockList blockList = client.QueryBlockDestination(new ClientProto.BlockList { BlockInfo = { blockInfoList } });

        //                long readOffset = -134217728;
        //                Task[] taskList = new Task[blockList.BlockInfo.Count];
        //                int index = 0;
        //                foreach (var blockInfo in blockList.BlockInfo)
        //                {
        //                    Task.Run(() =>
        //                    {
        //                        byte[] block = new byte[134217728];
        //                        readOffset += blockInfo.BlockSize;
        //                        Console.WriteLine(readOffset);
        //                        try
        //                        {
        //                            if (GetPipeLineReady(blockInfo, out ClientProto.ClientProto.ClientProtoClient writeClient))
        //                            {
        //                                if (client != null)
        //                                {
        //                                    Stopwatch watch = new Stopwatch();
        //                                    watch.Start();
        //                                    int bytesRead = 0;
        //                                    //responseStream.Position = readOffset;
        //                                    while (bytesRead < 134217728)
        //                                    {
        //                                        int length = Math.Min(blockInfo.BlockSize, blockInfo.BlockSize - bytesRead);
        //                                        //bytesRead += responseStream.Read(block, offset + bytesRead, length);
        //                                        bytesRead += responseStream.Read(block, bytesRead, length);
        //                                    }
        //                                    watch.Stop();
        //                                    Console.WriteLine("Total time to Read data from stream: " + watch.Elapsed);
        //                                    totalBytesRead += bytesRead;


        //                                    WriteBlock(writeClient, blockInfo, block).Wait();
        //                                }
        //                            }
        //                        }
        //                        catch (RpcException e)
        //                        {
        //                            totalBytesRead = response.ContentLength; // Stop reading
        //                            Console.WriteLine("Query for block destination failed: " + e.Message);
        //                        }
        //                    }).Wait();
        //                }
        //                responseStream.Flush();
        //            };
        //        }
        //        catch (AmazonS3Exception e)
        //        {
        //            Console.WriteLine("Exception message: {0}", e.Message);
        //        }
        //    }
        //}

        public static void ReadFileFromS3(ClientProto.ClientProto.ClientProtoClient client)
        {
            IAmazonS3 s3Client;
            using (s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
            {
                try
                {
                    long totalBytesRead = 0;
                    long s3BytseRead = 134217728;
                    //int blockSize = 134217728; // 128MB
                    int index = 0;
                    Task[] taskArray = new Task[8];
                    //BufferedStream[] responseStream = new BufferedStream[29];
                    //GetObjectResponse[] response = new GetObjectResponse[29];
                    using (StreamHolder responseStream = new StreamHolder())
                    {
                        while (s3BytseRead == 134217728 && index < 8)
                        {
                            // Get everything
                            GetObjectRequest request = new GetObjectRequest
                            {
                                BucketName = "wordcount-madiganz",
                                Key = "CC-MAIN-20180116070444-20180116090444-00000.warc",
                                ByteRange = new ByteRange(totalBytesRead, totalBytesRead + 134217727) // 128 MB,
                            };

                            //byte[] block = new byte[2097152];
                            //byte[] block = new byte[134217728];

                            //using (GetObjectResponse response = s3Client.GetObject(request))
                            //using (BufferedStream responseStream = new BufferedStream(response.ResponseStream, 2097152))
                            //GetObjectResponse response = s3Client.GetObject(request);
                            responseStream.Responses.Add(s3Client.GetObject(request));
                            //responseStream.Streams.Add(new BufferedStream(responseStream.Responses[index].ResponseStream, 4096));
                            responseStream.MemStreams.Add(new MemoryStream());

                            //responseStream.Responses[index].ResponseStream.CopyTo(responseStream.MemStreams[index]);
                            //BufferedStream responseStream = new BufferedStream(response.ResponseStream, 2097152);
                            //using (Stream responseStream = response.ResponseStream)
                            //{
                            if(responseStream.Responses[index].ContentLength == 0)
                            {
                                break;
                            }
                            s3BytseRead = responseStream.Responses[index].ContentLength;
                            totalBytesRead += responseStream.Responses[index].ContentLength;

                            Console.WriteLine("Bytes read: " + s3BytseRead);
                            Console.WriteLine("Total bytes read: " + totalBytesRead);

                            // Divide response into blocks
                            List<ClientProto.BlockInfo> blockInfoList = new List<ClientProto.BlockInfo>();
                            //for (long i = 0; i < response.ContentLength; i += 134217728)
                            //{
                            //int blockSize = 0;
                            //blockSize = ((i + 134217728) < response.ContentLength) ? 134217728 : (int)(response.ContentLength - i);
                            ClientProto.BlockInfo blockInfo = new ClientProto.BlockInfo
                            {
                                BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() },
                                //BlockSize = blockSize
                                BlockSize = (int)responseStream.Responses[index].ContentLength
                            };

                            //blockInfoList.Add(blockInfo);
                            //}

                            //ClientProto.BlockList blockList = client.QueryBlockDestination(new ClientProto.BlockList { BlockInfo = { blockInfoList } });
                            blockInfo = client.QueryBlockDestination(blockInfo);

                            //long readOffset = -134217728;
                            //Task[] taskArray = new Task[25];
                            //int index = 0;
                            //foreach (var blockInfo in blockList.BlockInfo)
                            //{
                            int streamIndex = index;
                            taskArray[index] = Task.Factory.StartNew(() =>
                            {
                                byte[] block = new byte[134217728];
                                //Stream stream = responseStream.Streams[streamIndex];
                                //readOffset += blockInfo.BlockSize;
                                //Console.WriteLine(readOffset);
                                try
                                {
                                    if (GetPipeLineReady(blockInfo, out ClientProto.ClientProto.ClientProtoClient writeClient))
                                    {
                                        if (client != null)
                                        {
                                            Stopwatch watch = new Stopwatch();
                                            watch.Start();
                                            int bytesRead = 0;
                                            //responseStream.Position = readOffset;
                                            //while (bytesRead < 134217728)
                                            //{
                                            //    int length = Math.Min(blockInfo.BlockSize, blockInfo.BlockSize - bytesRead);
                                            //    //bytesRead += responseStream.Read(block, offset + bytesRead, length);
                                            //    bytesRead += responseStream.Streams[streamIndex].Read(block, bytesRead, length);
                                            //}
                                            responseStream.Responses[streamIndex].ResponseStream.CopyTo(responseStream.MemStreams[streamIndex]);
                                            block = responseStream.MemStreams[streamIndex].ToArray();
                                            watch.Stop();
                                            Console.WriteLine("Total time to Read data from stream: " + watch.Elapsed);
                                            totalBytesRead += bytesRead;


                                            WriteBlock(writeClient, blockInfo, block).Wait();
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Failed to create pipeline");
                                    }
                                }
                                catch (NotSupportedException e)
                                {
                                    Console.WriteLine(e);
                                }
                                catch (RpcException e)
                                {
                                    totalBytesRead = responseStream.Responses[index].ContentLength; // Stop reading
                                    Console.WriteLine("Query for block destination failed: " + e.Message);
                                }
                                catch (OutOfMemoryException)
                                {
                                    Task.Delay(5000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            });
                            //}
                            //responseStream.Flush();
                            //};
                            index++;
                        }
                        //taskArray[0].Wait();
                        Task.WaitAll(taskArray);
                    }
                    //taskArray[0].Wait();
                    //Task.WaitAll(taskArray);
                    //foreach(var stream in responseStream)
                    //{
                    //    stream.Flush();
                    //    stream.Dispose();
                    //}
                }
                catch (AmazonS3Exception e)
                {
                    Console.WriteLine("Exception message: {0}", e.Message);
                }
            }
        }

        //public static void StartWritingData(ClientProto.ClientProto.ClientProtoClient client, byte[] block, int bytesRead)
        //{
        //    try
        //    {
        //        ClientProto.BlockInfo blockInfo = client.QueryBlockDestination(new ClientProto.BlockInfo { BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() }, BlockSize = bytesRead });

        //        if (GetPipeLineReady(blockInfo, out ClientProto.ClientProto.ClientProtoClient writeClient))
        //        {
        //            if (client != null)
        //            {
        //                WriteBlock(writeClient, blockInfo, block).Wait();
        //            }
        //        }
        //    }
        //    catch (RpcException e)
        //    {
        //        Console.WriteLine("Query for block destination failed: " + e.Message);
        //    }
        //}

        public static bool GetPipeLineReady(ClientProto.BlockInfo blockInfo, out ClientProto.ClientProto.ClientProtoClient client)
        {
            ClientProto.StatusResponse readyResponse = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            try
            {
                //Channel channel = new Channel(blockInfo.IpAddress[0] + ":" + "50051", ChannelCredentials.Insecure);
                Channel channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);
                //channel = new Channel("127.0.0.1" + ":" + "50052", ChannelCredentials.Insecure);


                client = new ClientProto.ClientProto.ClientProtoClient(channel);
                // readyResponse should always be Ready if it was able to connect
                //readyResponse = client.GetReady(new ClientProto.BlockInfo
                //{
                //    BlockId = new ClientProto.UUID { Value = blockId.ToString() },
                //    IpAddress = { addresses }
                //});

                readyResponse = client.GetReady(blockInfo);

                // Mainly for debugging
                if (readyResponse.Message != null)
                {
                    Console.WriteLine(readyResponse.Message);
                }
                return true;
                //WriteBlock(client, blockId).Wait();
                //WriteBlock(client, Guid.Parse(blockInfo.BlockId.Value)).Wait();
            }
            catch (RpcException e)
            {
                // Can't connect to first node -> Need to contact namenode or try other datanode
                Console.WriteLine("Get ready failed: " + e.Message);
                client = null;
                return false;
            }
        }

        public static async Task WriteBlock(ClientProto.ClientProto.ClientProtoClient client, ClientProto.BlockInfo blockInfo, byte[] block)
        {
            //Console.WriteLine("in WriteBLock call");


            //Metadata metaData = new Metadata { new Metadata.Entry("blockid", blockId.ToString()) };
            Metadata metaData = new Metadata {
                new Metadata.Entry("blockid", blockInfo.BlockId.Value),
                new Metadata.Entry("blocksize", blockInfo.BlockSize.ToString())
            };

            byte[] chunk = new byte[2097152];

            int totalBytesRead = 0;

            using (var call = client.WriteBlock(metaData))
            {
                bool dataNodeFailed = false;
                //Stopwatch watch = new Stopwatch();
                //watch.Start();

                while (totalBytesRead < blockInfo.BlockSize)
                {
                    int length = Math.Min(chunk.Length, blockInfo.BlockSize - totalBytesRead);

                    Buffer.BlockCopy(block, totalBytesRead, chunk, 0, length);

                    totalBytesRead += length;

                    try
                    {
                        //Console.WriteLine("writing block, size: " + length);
                        await call.RequestStream.WriteAsync(new ClientProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(chunk) });
                    }
                    catch (RpcException e)
                    {
                        dataNodeFailed = true;
                        totalBytesRead = blockInfo.BlockSize; // Stop reading
                        Console.WriteLine("Writing block failed: " + e);
                    }
                }

                ClientProto.StatusResponse resp = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                if (!dataNodeFailed)
                {
                    await call.RequestStream.CompleteAsync();
                    resp = await call.ResponseAsync;
                }
                Console.WriteLine(resp.Type.ToString());
                //watch.Stop();
                //Console.WriteLine("Total time to write: " + watch.Elapsed);
            };
        }
    }
}