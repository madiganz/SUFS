using ClientProto;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// Class to manage reading a file from SUFS
    /// </summary>
    static class FileReader
    {
        private static long FileSize;
        private static long BytesReadFromFile;

        /// <summary>
        /// Read a file from SUFS
        /// </summary>
        /// <param name="nameNodeClient">nameNodeClient</param>
        /// <param name="remotePath">Path of the file to be read in SUFS</param>
        /// <param name="localPath">Path of the file to be stored on client node</param>
        public static void ReadFile(ClientProto.ClientProto.ClientProtoClient nameNodeClient, string remotePath, string localPath)
        {
            var blockMessage = nameNodeClient.ReadFile(new ClientProto.Path { FullPath = remotePath });
            FileSize = blockMessage.FileSize;
            BytesReadFromFile = 0;

            if(FileSize == 0)
            {
                Console.WriteLine("Reading Failed: file size 0. File may not exist." );
                return;
            }

            var fileName = GetFileName(remotePath);

            localPath += "/";

            localPath += fileName;
            

            var writerStream = new FileStream(localPath, FileMode.Append, FileAccess.Write);

            foreach (var blockInfo in blockMessage.BlockInfo)
            {
                WriteToFile(blockInfo, writerStream);
            }
            writerStream.Flush();

        }
        
        /// <summary>
        /// Get the filename from FilePath
        /// </summary>
        /// <param name="path">Remote file path in SUFS</param>
        private static string GetFileName(string path)
        {
            
            //var array = path.Split(@"\");
            
            Char delimiter = '/';
            var array = path.Split(delimiter);
            
            return array[array.Length - 1];
        }
        
        /// <summary>
        /// Stream version write to file
        /// </summary>
        private static void WriteToFile(BlockInfo blockInfo, FileStream writerStream)
        {
            bool success = false;

            foreach (var dataNodeIp in blockInfo.IpAddress)
            {
                try
                {
                    var channel = new Channel(dataNodeIp + ":50051", ChannelCredentials.Insecure);
                    var dataNodeClient = new ClientProto.ClientProto.ClientProtoClient(channel);
                   
                    using (var call = dataNodeClient.ReadBlock(blockInfo.BlockId))
                    {
                        int copiedLength = 0;

                        while (call.ResponseStream.MoveNext().Result)
                        {
                            var bytes = call.ResponseStream.Current.Data.ToByteArray();
                            WriteToFile(bytes, writerStream);

                            copiedLength += bytes.Length;
                        }

                        if (copiedLength > 0)
                        {
                            success = true;
                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error when connecting to data node: {dataNodeIp}. Exception: {exception.Message}. Trying next.");
                }
            }

            if (!success)
            {
                throw new Exception($"Cannot get block {blockInfo.BlockId.Value}");
            }
        }
        
        /// <summary>
        /// Write byte array to FileStream
        /// </summary>
        /// <param name="blockData">byte array</param>
        /// <param name="writerStream">File Stream to store byte array</param>
        private static void WriteToFile(byte[] blockData, FileStream writerStream)
        {
            BytesReadFromFile += blockData.Length;
            // Casts are very necessary here!
            Console.Write("\rDownloading {0}", (((double)BytesReadFromFile / (double)FileSize)).ToString("0.00%"));
            writerStream.Write(blockData, 0, blockData.Length);
        }
    }
}
