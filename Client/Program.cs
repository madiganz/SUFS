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
            // validate number of argumens
			if (args.Length < 2)
			{
				System.Console.WriteLine("Please enter 2 arguments.");
				System.Console.WriteLine("Usage: Client <action> <path>");
			}

			string action = args[0];
			string path = args[1];
			

			// get connected to server
			Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
			var client = new ClientProto.ClientProto.ClientProtoClient(channel);

			// judge what action is taken
			if (action == "DeleteDirectory")
			{
				var reply = client.DeleteDirectory(new ClientProto.Path { Fullpath = path });
				Console.WriteLine("Delete directory action: " + reply);
			}

			else if (action == "CreateDirectory")
			{
				var reply = client.CreateDirectory(new ClientProto.Path { Fullpath = path });
				Console.WriteLine("Create directory action: " + reply);
			}

			else if (action == "AddFile")
			{
				string address = args[2];
				var reply = client.AddFile(new ClientProto.NewFile { Fullpath = path, Address = address });
				Console.WriteLine("Add file action: " + reply);
			}

			else if (action == "DeleteFile")
			{
				var reply = client.DeleteFile(new ClientProto.Path { Fullpath = path });
				Console.WriteLine("Delete file action: " + reply);
			}

			else if (action == "TestFile")
			{
				var reply = client.TestFile(new ClientProto.Path { Fullpath = path });
				Console.WriteLine("Test file action: " + reply);
			}

			else if (action == "ReadFile")
			{
				var reply = client.ReadFile(new ClientProto.Path { Fullpath = path });

				Console.WriteLine("Read file action: " + reply);
			}

			else if (action == "RenameFile")
			{
				var reply = client.RenameFile(new ClientProto.Path { Fullpath = path });
				Console.WriteLine("Rename file action: " + reply);
			}

			else if (action == "MoveFile")
			{
				string newpath = args[2];
				var reply = client.MoveFile(new ClientProto.DoublePath { Fullpath = path, Newpath = newpath });
				Console.WriteLine("Move file action: " + reply);
			}

			channel.ShutdownAsync().Wait();

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
            
            
            
            
            
            
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
