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

            /*API between Client and NameNode*/
            if (args.Length < 3)
            {
                System.Console.WriteLine("Please enter 3 arguments.");
                System.Console.WriteLine("Usage: Client <namenode ip> <action> <path>");
            }

            string ip = args[0];
            string action = args[1];
            string path = args[2];


            // get connected to server
            Channel channel = new Channel(ip + ":50051", ChannelCredentials.Insecure);
            var client = new ClientProto.ClientProto.ClientProtoClient(channel);

            // judge what action is taken
            if (action == "DeleteDirectory")
            {
                var reply = client.DeleteDirectory(new ClientProto.Path { FullPath = path });
                Console.WriteLine("Delete directory action: " + reply);
            }

            else if (action == "CreateDirectory")
            {
                var reply = client.CreateDirectory(new ClientProto.Path { FullPath = path });
                Console.WriteLine("Create directory action: " + reply);
            }

            else if (action == "CreateFile")
            {
                try
                {
                    FileCreater fileCreater = new FileCreater(client);

                    if (args.Length > 2)
                    {
                        fileCreater.CreateFile(path, args[3]);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception while creating file: ", e);
                }
            }

            else if (action == "DeleteFile")
            {
                var reply = client.DeleteFile(new ClientProto.Path { FullPath = path });
                Console.WriteLine("Delete file action: " + reply);
            }

            else if (action == "ReadFile")
            {
                var localPath = Directory.GetCurrentDirectory();

                FileReader.ReadFile(client, path, localPath);
            }

            else if (action == "MoveFile")
            {
                string newpath = args[3];
                var reply = client.MoveFile(new ClientProto.DoublePath { Fullpath = path, Newpath = newpath });
                Console.WriteLine("Move file action: " + reply);
            }
            
            else if(action == "ListNodes")
            {
                var reply = client.ListNodes(new ClientProto.Path { FullPath = path });
                Console.WriteLine("List of block replicass for the file are: " + reply);
            }
            
            else if(action == "ListContents")
            {
                var reply = client.ListContents(new ClientProto.Path { FullPath = path });
                Console.WriteLine("List of directory contents are: " + reply);
            }
            
            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
