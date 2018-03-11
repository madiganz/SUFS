using ClientProto;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// Class to manage reading a file from SUFS
    /// </summary>
    static class FileReader
    {
        /// <summary>
        /// Read a file from SUFS
        /// </summary>
        /// <param name="nameNodeClient">nameNodeClient</param>
        /// <param name="remotePath">Path of the file to be read in SUFS</param>
        /// <param name="localPath">Path of the file to be stored on client node</param>
        public static void ReadFile(ClientProto.ClientProto.ClientProtoClient nameNodeClient, string remotePath, string localPath)
        {
            var blockMessage = nameNodeClient.ReadFile(new ClientProto.Path { FullPath = remotePath });

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
        /// Read a file from SUFS and write to a local file
        /// </summary>
        /// <param name="blockInfo">The list of blocks to be read</param>
        /// <param name="writerStream">The fileStream to store the read data</param>
        private static void WriteToFile(BlockInfo blockInfo, FileStream writerStream)
        {
            bool success = false;

            foreach (var dataNodeIp in blockInfo.IpAddress)
            {
                try
                {
                    var channel = new Channel(dataNodeIp + ":50051", ChannelCredentials.Insecure);
                    var dataNodeClient = new ClientProto.ClientProto.ClientProtoClient(channel);

                    var bytesString = dataNodeClient.ReadBlock(blockInfo.BlockId);
                    var bytes = bytesString.ToByteArray();
                    WriteToFile(bytes, writerStream);

                    success = true;

                    break;
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
            writerStream.Write(blockData, 0, blockData.Length);
        }
    }
}
