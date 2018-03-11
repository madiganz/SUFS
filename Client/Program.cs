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

            if (args.Length < 1)
            {
                System.Console.WriteLine("Usage: Client <namenode ip>");
                return;
            }

            string ip = args[0];

            // get connected to server
            Channel channel = new Channel(ip + ":50051", ChannelCredentials.Insecure);
            var client = new ClientProto.ClientProto.ClientProtoClient(channel);

            while (true)
            {
                // Ask user to enter action to perform
                Console.WriteLine("Enter an action to perform (help for more information, quit to end)?");
                var line = Console.ReadLine();

                var lineData = line.Split(" ");
                if (lineData.Length == 0)
                {
                    Console.WriteLine("Enter an action to perform (help for more information, quit to end)?");
                }

                string action = lineData[0];

                // Quit
                if (line == "quit")
                {
                    channel.ShutdownAsync().Wait();
                    return;
                }

                else if (line == "help")
                {
                    Console.WriteLine("Possible Actions: ");
                    Console.WriteLine("Create a file: createfile <full path of file to be created> <location of file> <local/s3>");
                    Console.WriteLine("Read a file: readfile <file path on SUFS>");
                    Console.WriteLine("Delete a file: deletefile <full path of file on SUFS>");
                    Console.WriteLine("Create a directory: createdirectory <full path of directory on SUFS>");
                    Console.WriteLine("Delete a directory: deletedirectory <full path of directory on SUFS>");
                    Console.WriteLine("Move a file: movefile <full path of file on SUFS> <new full path>");
                    Console.WriteLine("List a files blocks and their locations: listnodes <full path of file on SUFS>");
                    Console.WriteLine("List contents of a directory: listcontents <full path of directory on SUFS>");
                    Console.WriteLine("Quit program: Quit");
                    Console.WriteLine();
                }

                else if (action.ToLower() == "deletedirectory")
                {
                    if (lineData.Length != 2)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'DeleteDirectory <full path of directory on SUFS>'");
                        Console.WriteLine();
                        continue;
                    }
                    string path = lineData[1];
                    var reply = client.DeleteDirectory(new ClientProto.Path { FullPath = path });
                    Console.WriteLine("Delete directory: " + reply.Type.ToString());
                }

                else if (action.ToLower() == "createdirectory")
                {
                    if (lineData.Length != 2)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'CreateDirectory <full path of directory on SUFS>'");
                        Console.WriteLine();
                        continue;
                    }
                    string path = lineData[1];
                    var reply = client.CreateDirectory(new ClientProto.Path { FullPath = path });
                    Console.WriteLine("Create directory: " + reply.Type.ToString());
                }

                else if (action.ToLower() == "createfile")
                {
                    if (lineData.Length != 4)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'CreateFile <full path of file to be created> <location of file> <local/s3>'");
                        Console.WriteLine();
                        continue;
                    }
                    try
                    {
                        string path = lineData[1];
                        FileCreater fileCreater = new FileCreater(client);
                        fileCreater.CreateFile(path, lineData[2], lineData[3]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception while creating file: ", e);
                    }
                }

                else if (action.ToLower() == "deletefile")
                {
                    if (lineData.Length != 2)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'DeleteFile <full path of file on SUFS>'");
                        Console.WriteLine();
                        continue;
                    }
                    string path = lineData[1];
                    var reply = client.DeleteFile(new ClientProto.Path { FullPath = path });
                    Console.WriteLine("Delete file action: " + reply.Type.ToString());
                }

                else if (action.ToLower() == "readfile")
                {
                    if (lineData.Length != 2)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'ReadFile <file path on SUFS>'");
                        Console.WriteLine();
                        continue;
                    }
                    string path = lineData[1];
                    var localPath = Directory.GetCurrentDirectory();
                    FileReader.ReadFile(client, path, localPath);
                    Console.WriteLine();
                    Console.WriteLine("Done reading file");
                }

                else if (action.ToLower() == "movefile")
                {
                    if (lineData.Length != 3)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'MoveFile <full path of file on SUFS> <new full path>'");
                        Console.WriteLine();
                        continue;
                    }
                    string path = lineData[1];
                    string newpath = lineData[2];
                    var reply = client.MoveFile(new ClientProto.DoublePath { Fullpath = path, Newpath = newpath });
                    Console.WriteLine("Move file action: " + reply.Type.ToString());
                }

                else if (action.ToLower() == "listnodes")
                {
                    if (lineData.Length != 2)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'ListNodes <full path of file on SUFS>'");
                        Console.WriteLine();
                        continue;
                    }
                    string path = lineData[1];
                    var reply = client.ListNodes(new ClientProto.Path { FullPath = path });
                    PrettyPrintNodeList(reply);
                }

                else if (action.ToLower() == "listcontents")
                {
                    if (lineData.Length != 2)
                    {
                        Console.WriteLine("Invalid arguments, input must be 'ListContents <full path of directory on SUFS>'");
                        Console.WriteLine();
                        continue;
                    }
                    string path = lineData[1];
                    var reply = client.ListContents(new ClientProto.Path { FullPath = path });
                    Console.WriteLine("List of directory contents:");
                    foreach (var s in reply.FileName)
                    {
                        Console.WriteLine(s);
                    }
                    Console.WriteLine();
                }

                else
                {
                    Console.WriteLine("Not a valid action.");
                }
            }
        }

        public static void PrettyPrintNodeList(ClientProto.ListOfNodesList listOfNodesList)
        {
            Console.WriteLine("List of block replicas for the file are:");
            foreach (var block in listOfNodesList.ListOfNodes)
            {
                Console.WriteLine("BlockID: " + block.BlockId);
                foreach (var node in block.NodeId)
                {
                    Console.WriteLine("\tDataNode: " + node);
                }
            }
            Console.WriteLine();
        }
    }
}
