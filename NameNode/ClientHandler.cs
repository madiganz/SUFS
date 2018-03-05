using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameNode
{
    class ClientHandler : ClientProto.ClientProto.ClientProtoBase
    {

        /* API between Client and Namenode */

        public override Task<ClientProto.StatusResponse> CreateDirectory(ClientProto.Path path, ServerCallContext context)
        {
            Console.WriteLine(path);
            return Task.FromResult(new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success });
        }

        public override Task<ClientProto.StatusResponse> CreateFile(ClientProto.Path file, ServerCallContext context)
        {
            var response = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Ok };
            if (Program.Database.FileExists(file.FullPath))
            {
                response = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.FileExists };
            }
            return Task.FromResult(response);
        }

        public override Task<ClientProto.StatusResponse> DeleteFile(ClientProto.Path path, ServerCallContext context)
        {
            Console.WriteLine(path);
            return Task.FromResult(new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success });
        }

        public override Task<ClientProto.StatusResponse> RenameFile(ClientProto.Path path, ServerCallContext context)
        {
            Console.WriteLine(path);
            return Task.FromResult(new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success });
        }

        public override Task<ClientProto.StatusResponse> MoveFile(ClientProto.DoublePath path, ServerCallContext context)
        {
            Console.WriteLine(path);
            return Task.FromResult(new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success });
        }

        //public override Task<ClientProto.Test> TestFile(ClientProto.Path path, ServerCallContext context)
        //{
        //	Console.WriteLine(path);
        //	return Task.FromResult(new ClientProto.Test { Test_ = { "1", "@" } });
        //}

        public override Task<ClientProto.BlockMessage> ReadFile(ClientProto.Path path, ServerCallContext context)
        {
            Console.WriteLine(path);
            List<ClientProto.BlockInfo> blockMessage = new List<ClientProto.BlockInfo>();
            ClientProto.UUID id = new ClientProto.UUID { Value = Guid.NewGuid().ToString() };
            List<string> ip = new List<string>();
            ip.Add("1");
            ip.Add("2");

            ClientProto.BlockInfo blockInfo = new ClientProto.BlockInfo { BlockId = id, IpAddress = { "1", "2" } };


            blockMessage.Add(blockInfo);
            blockMessage.Add(blockInfo);

            foreach (ClientProto.BlockInfo block in blockMessage)
                Console.WriteLine(block);

            return Task.FromResult(new ClientProto.BlockMessage { BlockInfo = { blockMessage } });
        }

        /* Below are original part */

        public override Task<ClientProto.StatusResponse> DeleteDirectory(ClientProto.Path path, ServerCallContext context)
        {
            if (Program.Database.DeleteDirectory(path.FullPath))
                return Task.FromResult(new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success });
            else
                return Task.FromResult(new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail });
        }

        //?
        public override Task<ClientProto.StatusResponse> AddDirectory(ClientProto.Path path, ServerCallContext context)
        {
            Program.Database.CreateDirectory(path.FullPath);
            return Task.FromResult(new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success });
        }


        //public override Task<ListOfNodes> ListNodes(ClientProto.Path path, ServerCallContext context)
        //{
        //    Program.Database.ListDataNodesStoringReplicas();
        //}

        public override Task<ClientProto.ListOfContents> ListContents(ClientProto.Path path, ServerCallContext context)
        {
            List<String> filename = new List<String>();
            filename = Program.Database.ListDirectoryContents(path.FullPath);
            ClientProto.ListOfContents contents = new ClientProto.ListOfContents
            {
                FileName = { filename }
            };
            return Task.FromResult(contents);
        }

        public override Task<ClientProto.BlockInfo> QueryBlockDestination(ClientProto.BlockInfo blockInfo, ServerCallContext context)
        {
            ClientProto.BlockInfo blckInfo = new ClientProto.BlockInfo
            {
                BlockId = blockInfo.BlockId,
                BlockSize = blockInfo.BlockSize,
                IpAddress = { DataNodeManager.Instance.GetDataNodesForReplication() }
            };

            return Task.FromResult(blckInfo);
        }
    }
}