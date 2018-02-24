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
                ReadFileFromS3(client);
            }
            else
            {
                Console.WriteLine("File already exists!");
            }
        }

        public static void ReadFileFromS3(ClientProto.ClientProto.ClientProtoClient client)
        {
            IAmazonS3 s3Cient;
            using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = "wordcount-madiganz",
                    Key = "CC-MAIN-20180116070444-20180116090444-00000.warc"
                    //ByteRange = new ByteRange(0, 134217727) // 128 MB,
                };

                //byte[] block = new byte[2097152];
                byte[] block = new byte[134217727];

                long totalBytesRead = 0;

                using (GetObjectResponse response = s3Cient.GetObject(request))
                using (Stream responseStream = response.ResponseStream)
                {
                    long totalBytesToRead = response.ContentLength;
                    while (totalBytesRead < response.ContentLength)
                    {
                        int numBytesRead = 0;
                        if (totalBytesToRead < 134217727)
                        {
                            int numBytesToRead = (int)totalBytesToRead;
                            do
                            {
                                int n = responseStream.Read(block, numBytesRead, numBytesToRead);

                                numBytesRead += n;
                                numBytesToRead -= n;
                                totalBytesRead += n;
                            } while (numBytesToRead > 0);
                        }
                        else
                        {
                            int numBytesToRead = 134217727;
                            do
                            {
                                int n = responseStream.Read(block, numBytesRead, numBytesToRead);
                                numBytesRead += n;
                                numBytesToRead -= n;
                                totalBytesRead += n;
                            } while (numBytesToRead > 0);
                        }

                        try
                        {
                            ClientProto.BlockInfo blockInfo = client.QueryBlockDestination(new ClientProto.BlockInfo { BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() }, BlockSize = numBytesRead });
                            Console.WriteLine(blockInfo);
                            if (GetPipeLineReady(blockInfo, out ClientProto.ClientProto.ClientProtoClient writeClient))
                            {
                                if (client != null)
                                {
                                    WriteBlock(writeClient, blockInfo, block).Wait();
                                }
                            }
                        }
                        catch (RpcException e)
                        {
                            totalBytesRead = response.ContentLength; // Stop reading
                            Console.WriteLine("Query for block destination failed: " + e.Message);
                        }
                    }

                    responseStream.Flush();
                    responseStream.Dispose();
                    response.Dispose();
                };
            }
        }

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
            Console.WriteLine("in WriteBLock call");


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
                Stopwatch watch = new Stopwatch();
                watch.Start();

                while (totalBytesRead < blockInfo.BlockSize)
                {
                    int length = Math.Min(chunk.Length, blockInfo.BlockSize - totalBytesRead);

                    // Changed from Array.Copy as per Marc's suggestion
                    Buffer.BlockCopy(block, totalBytesRead, chunk, 0, length);

                    totalBytesRead += length;

                    try
                    {
                        Console.WriteLine("writing block, size: " + length);
                        await call.RequestStream.WriteAsync(new ClientProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(chunk) });
                    }
                    catch (RpcException e)
                    {
                        dataNodeFailed = true;
                        totalBytesRead = blockInfo.BlockSize; // Stop reading
                        Console.WriteLine("Writing block failed: " + e);
                        call.Dispose();
                    }
                }

                ClientProto.StatusResponse resp = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                if (!dataNodeFailed)
                {
                    await call.RequestStream.CompleteAsync();
                    resp = await call.ResponseAsync;
                }
                Console.WriteLine(resp.Type.ToString());
                watch.Stop();
                Console.WriteLine("Total time to write: " + watch.Elapsed);
            };
        }
    }

    //public static async Task WriteBlock(ClientProto.ClientProto.ClientProtoClient client, Guid blockId)
    //{
    //    Console.WriteLine("in WriteBLock call");
    //    IAmazonS3 s3Cient;
    //    using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
    //    {
    //        GetObjectRequest request = new GetObjectRequest
    //        {
    //            BucketName = "wordcount-madiganz",
    //            Key = "CC-MAIN-20180116070444-20180116090444-00000.warc",
    //            ByteRange = new ByteRange(0, 134217727) // 128 MB
    //        };

    //        Metadata metaData = new Metadata { new Metadata.Entry("blockid", blockId.ToString()) };

    //        byte[] block = new byte[2097152];
    //        //byte[] block = new byte[4096];

    //        long totalBytesRead = 0;

    //        using (var call = client.WriteBlock(metaData))
    //        {
    //            bool dataNodeFailed = false;
    //            Stopwatch watch = new Stopwatch();
    //            watch.Start();

    //            using (GetObjectResponse response = s3Cient.GetObject(request))
    //            using (Stream responseStream = response.ResponseStream)
    //            {
    //                long totalBytesToRead = response.ContentLength;
    //                while (totalBytesRead < response.ContentLength)
    //                {
    //                    int numBytesRead = 0;
    //                    if (totalBytesToRead < 2097152)
    //                    {
    //                        int numBytesToRead = (int)totalBytesToRead;
    //                        do
    //                        {
    //                            int n = responseStream.Read(block, numBytesRead, numBytesToRead);

    //                            numBytesRead += n;
    //                            numBytesToRead -= n;
    //                            totalBytesRead += n;
    //                        } while (numBytesToRead > 0);
    //                    }
    //                    else
    //                    {
    //                        int numBytesToRead = 2097152;
    //                        do
    //                        {
    //                            int n = responseStream.Read(block, numBytesRead, numBytesToRead);
    //                            numBytesRead += n;
    //                            numBytesToRead -= n;
    //                            totalBytesRead += n;
    //                        } while (numBytesToRead > 0);
    //                    }

    //                    try
    //                    {
    //                        Console.WriteLine("writing block, size: " + numBytesRead + ", remaining: " + (response.ContentLength - totalBytesRead));
    //                        await call.RequestStream.WriteAsync(new ClientProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(block) });
    //                    }
    //                    catch (RpcException e)
    //                    {
    //                        dataNodeFailed = true;
    //                        totalBytesRead = response.ContentLength; // Stop reading
    //                        Console.WriteLine("Writing block failed: " + e.Message);
    //                        call.Dispose();
    //                    }
    //                }

    //                ClientProto.StatusResponse resp = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
    //                if (!dataNodeFailed)
    //                {
    //                    await call.RequestStream.CompleteAsync();
    //                    resp = await call.ResponseAsync;
    //                }
    //                Console.WriteLine(resp.Type.ToString());
    //                watch.Stop();
    //                Console.WriteLine("Total time to write: " + watch.Elapsed);
    //                responseStream.Flush();
    //                responseStream.Dispose();
    //                response.Dispose();
    //            };
    //        }
    //    }
    //}
}