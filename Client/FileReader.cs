using ClientProto;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
    static class FileReader
    {
        public static void ReadFile(ClientProto.ClientProto.ClientProtoClient nameNodeClient, string remotePath, string localPath)
        {
            var blockMessage = nameNodeClient.ReadFile(new ClientProto.Path { FullPath = remotePath });

            var fileName = GetFileName(remotePath);

            if (localPath.EndsWith('\\'))
            {
                localPath += fileName;
            }

            var writerStream = new FileStream(localPath, FileMode.CreateNew, FileAccess.Write);

            foreach (var blockInfo in blockMessage.BlockInfo)
            {
                WriteToFile(blockInfo, writerStream);
            }
        }

        private static string GetFileName(string path)
        {
            var array = path.Split(@"\");

            return array[array.Length - 1];
        }

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

        private static void WriteToFile(byte[] blockData, FileStream writerStream)
        {
            //// TODO
        }
    }
}
