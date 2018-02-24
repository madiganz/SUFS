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

            if (task == "write")
            {
                //ClientProto.BlockMessage blockMessage = client.CreateFile(new ClientProto.NewFile { Fullpath = "path" });
                //foreach(var blockInfo in blockMessage.BlockInfo)
                //{
                //    WriteBlock(client, blockInfo).Wait();
                //}
                List<string> addresses = new List<string>
                {
                    "172.31.40.133"
                };
                ClientProto.StatusResponse readyResponse = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
                Guid blockId = Guid.NewGuid();
                try
                {
                    readyResponse = client.GetReady(new ClientProto.BlockInfo
                    {
                        BlockId = new ClientProto.UUID { Value = blockId.ToString() },
                        IpAddress = { addresses }
                    });
                }
                catch
                {
                    // Can't connect to first node -> Need to contact namenode or try other datanode
                    Console.WriteLine("Get Ready Failed");
                }
                if (readyResponse.Type == ClientProto.StatusResponse.Types.StatusType.Ready)
                {
                    WriteBlock(client, blockId).Wait();
                }
                else
                {
                    // Other nodes couldn't connect
                    Console.WriteLine("ready failed");
                }
            }

            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task WriteBlock(ClientProto.ClientProto.ClientProtoClient client, Guid blockId)
        {
            Console.WriteLine("in WriteBLock call");
            IAmazonS3 s3Cient;
            using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = "wordcount-madiganz",
                    Key = "CC-MAIN-20180116070444-20180116090444-00000.warc",
                    ByteRange = new ByteRange(0, 134217727) // 128 MB
                };

                Metadata metaData = new Metadata { new Metadata.Entry("blockid", blockId.ToString()) };
                
                byte[] block = new byte[2097152];
                //byte[] block = new byte[4096];
                long totalBytesRead = 0;

                using (var call = client.WriteBlock(metaData))
                {
                    bool dataNodeFailed = false;
                    Console.WriteLine("call established " + DateTime.UtcNow);
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    using (GetObjectResponse response = s3Cient.GetObject(request))
                    using (Stream responseStream = response.ResponseStream)
                    {
                        long totalBytesToRead = response.ContentLength;
                        while (totalBytesRead < response.ContentLength)
                        {
                            int numBytesRead = 0;
                            if (totalBytesToRead < 2097152)
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
                                int numBytesToRead = 2097152;
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
                                Console.WriteLine("writing block, size: " + numBytesRead + ", remaining: " + (response.ContentLength - totalBytesRead));
                                await call.RequestStream.WriteAsync(new ClientProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(block) });
                            }
                            catch
                            {
                                dataNodeFailed = true;
                                totalBytesRead = response.ContentLength; // Stop reading
                                Console.WriteLine("Writing block failed");
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
        }
    }
}