using Amazon.S3;
using Amazon.S3.Model;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        public static void Main(string[] args)
        {
            // FOr testing
            string IpAddress = args[0];
            Channel channel = new Channel(IpAddress + ":50051", ChannelCredentials.Insecure);

            var client = new ClientProto.ClientProto.ClientProtoClient(channel);

            //String user = "Tong";

            //var reply = client.DeleteDirectory( new ClientProto.Path { Fullpath = "Path" } );

            //Console.WriteLine("Greeting: " + reply);

            //IAmazonS3 s3Cient;
            //using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
            //{
            //    //GetObjectRequest request = new GetObjectRequest
            //    //{
            //    //    BucketName = "seattleu-cloud-computing",
            //    //    Key = "common-crawl",
            //    //    ByteRange = new ByteRange(0, 128)
            //    //};
            //    GetObjectRequest request = new GetObjectRequest
            //    {
            //        BucketName = "wordcount-madiganz",
            //        Key = "CC-MAIN-20180116070444-20180116090444-00000.warc",
            //        ByteRange = new ByteRange(0, 127)
            //    };


            //    using (GetObjectResponse response = s3Cient.GetObject(request))
            //    using (Stream responseStream = response.ResponseStream)
            //    {
            //        byte[] block = new byte[128];
            //        responseStream.Read(block, 0, 127);

            //        //This should be received from namenode
            //        ClientProto.DataNode node = new ClientProto.DataNode
            //        {
            //            Id = new ClientProto.UUID { Value = Guid.NewGuid().ToString() },
            //            IpAddress = "172.31.44.5"
            //        };

            //        ClientProto.DataBlock dataBlock = new ClientProto.DataBlock
            //        {
            //            BlockId = new ClientProto.UUID { Value = Guid.NewGuid().ToString() },
            //            Data = Google.Protobuf.ByteString.CopyFrom(block),
            //            DataNodes = { node }
            //        };

            //        List<string> addresses = new List<string>
            //        {
            //            "172.31.44.5"
            //        };

            //        Metadata metaData = new Metadata
            //        {
            //            new Metadata.Entry("blockid", Guid.NewGuid().ToString()),
            //            new Metadata.Entry("ipaddresses", String.Join(",", addresses.ToArray()))
            //        };

            //        using (var call = client.WriteBlock(metaData))
            //        {
            //            foreach (var b in block)
            //            {
            //                await call.RequestStream.WriteAsync(new ClientProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(b)} );
            //            }
            //            await call.RequestStream.CompleteAsync();

            //            ClientProto.StatusResponse resp = await call.ResponseAsync;
            //            Console.WriteLine(resp.Type.ToString());
            //        }
            //        //var reply = await client.WriteBlock(dataBlock, metaData);
            //        //Console.WriteLine(resp.Type.ToString());
            //    };
            //}
            WriteBlock(client).Wait();

            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task WriteBlock(ClientProto.ClientProto.ClientProtoClient client)
        {
            Console.WriteLine("in WriteBLock call");
            IAmazonS3 s3Cient;
            using (s3Cient = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
            {
                //GetObjectRequest request = new GetObjectRequest
                //{
                //    BucketName = "seattleu-cloud-computing",
                //    Key = "common-crawl",
                //    ByteRange = new ByteRange(0, 128)
                //};
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = "wordcount-madiganz",
                    Key = "CC-MAIN-20180116070444-20180116090444-00000.warc",
                    ByteRange = new ByteRange(0, 134217727) // 128 MB
                };

                //List<string> addresses = new List<string>();
                List<string> addresses = new List<string>
                {
                    "172.31.44.5"
                };

                Metadata metaData = new Metadata
                    {
                        new Metadata.Entry("blockid", Guid.NewGuid().ToString()),
                        new Metadata.Entry("ipaddresses", String.Join(",", addresses.ToArray()))
                    };
                byte[] block = new byte[4096];
                long totalBytesRead = 0;

                using (var call = client.WriteBlock(metaData))
                {
                    //Console.WriteLine("call initialized");

                    using (GetObjectResponse response = s3Cient.GetObject(request))
                    using (Stream responseStream = response.ResponseStream)
                    {
                        long totalBytesToRead = response.ContentLength;
                        while (totalBytesRead < response.ContentLength)
                        {
                            int numBytesRead = 0;
                            if (totalBytesToRead < 4096)
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
                                int numBytesToRead = 4096;
                                do
                                {
                                    int n = responseStream.Read(block, numBytesRead, numBytesToRead);
                                    numBytesRead += n;
                                    numBytesToRead -= n;
                                    totalBytesRead += n;
                                } while (numBytesToRead > 0);
                            }

                            Console.WriteLine("writing block, size: " + numBytesRead + ", remaining: " + (response.ContentLength - totalBytesRead));

                            await call.RequestStream.WriteAsync(new ClientProto.BlockData { Data = Google.Protobuf.ByteString.CopyFrom(block) });

                            await call.RequestStream.CompleteAsync();

                            ClientProto.StatusResponse resp = await call.ResponseAsync;
                            Console.WriteLine(resp.Type.ToString());
                        }
                        //var reply = await client.WriteBlock(dataBlock, metaData);
                        //Console.WriteLine(resp.Type.ToString());
                    };

                }
            }
        }
    }
}

