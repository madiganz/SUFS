﻿using Amazon.S3;
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
            //// Assume passed in parameters:
            //// NameNodeIP:Port => args[0],
			      Channel channel = new Channel(args[0], ChannelCredentials.Insecure);
            
            // Use "write" to write blocks to nodes. WriteBlock function takes some manual config
            string task = args[1];
            //Channel channel = new Channel(IpAddress + ":50051", ChannelCredentials.Insecure);
            //Channel channel = new Channel("127.0.0.1" + ":50051", ChannelCredentials.Insecure);

            var client = new ClientProto.ClientProto.ClientProtoClient(channel);

			      var reply = client.DeleteDirectory( new ClientProto.Path { Fullpath = "Path" } );

            if (task == "write")
            {
                WriteBlock(client).Wait();
            }

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
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = "wordcount-madiganz",
                    Key = "CC-MAIN-20180116070444-20180116090444-00000.warc",
                    ByteRange = new ByteRange(0, 134217727) // 128 MB
                };

                //List<string> addresses = new List<string>();
                List<string> addresses = new List<string>
                {
                    "172.31.40.133"
                };

                Metadata metaData = new Metadata
                    {
                        new Metadata.Entry("blockid", Guid.NewGuid().ToString()),
                        new Metadata.Entry("ipaddresses", String.Join(",", addresses.ToArray()))
                    };
                //4096
                //byte[] block = new byte[2097152];
                byte[] block = new byte[4096];
                long totalBytesRead = 0;

                using (var call = client.WriteBlock(metaData))
                {
                    //Console.WriteLine("call initialized");
                    //using (Stream responseStream = new FileStream(@"C:\Users\Zach Madigan\Documents\Cloud Computing\CC-MAIN-20180116070444-20180116090444-00000.warc", FileMode.Open, FileAccess.Read))
                    using (GetObjectResponse response = s3Cient.GetObject(request))
                    using (Stream responseStream = response.ResponseStream)
                    {
                        long totalBytesToRead = response.ContentLength;
                        while (totalBytesRead < response.ContentLength)
                        //long totalBytesToRead = 134217727;
                        //while (totalBytesRead < 134217727)
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

                            //await call.RequestStream.CompleteAsync();

                            //ClientProto.StatusResponse resp = await call.ResponseAsync;
                            //Console.WriteLine(resp.Type.ToString());
                        }

                        await call.RequestStream.CompleteAsync();

                        ClientProto.StatusResponse resp = await call.ResponseAsync;
                        Console.WriteLine(resp.Type.ToString());
                        //var reply = await client.WriteBlock(dataBlock, metaData);
                        //Console.WriteLine(resp.Type.ToString());
                    };

                }
            }
        }
    }
}