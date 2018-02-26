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

            //Channel channel = new Channel(IpAddress + ":50051", ChannelCredentials.Insecure);
            //Channel channel = new Channel("127.0.0.1" + ":50051", ChannelCredentials.Insecure);

            var client = new ClientProto.ClientProto.ClientProtoClient(channel);

            //var reply = client.DeleteDirectory(new ClientProto.Path { Fullpath = "Path" });

            // Use "create" to write blocks to nodes. WriteBlock function takes some manual config
            // "create local" or create s3"
            if (args.Length > 1)
            {
                string task = args[1];
                if (task == "create")
                {
                    FileCreater fileCreater = new FileCreater(client);
                    if (args.Length > 2)
                    {
                        fileCreater.CreateFile(args[2]);
                    }
                    else
                    {
                        fileCreater.CreateFile();
                    }
                }
            }

            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}