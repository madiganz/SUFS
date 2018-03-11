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
            return Task.FromResult(Program.Database.CreateDirectory(path));
        }

        public override Task<ClientProto.StatusResponse> CreateFile(ClientProto.Path file, ServerCallContext context)
        {
            //var response = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Ok };
            //if (Program.Database.FileExists(file.FullPath))
            //{
            //    response = new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.FileExists };
            //}
            return Task.FromResult(Program.Database.CreateFile(file));
        }

        public override Task<ClientProto.StatusResponse> DeleteFile(ClientProto.Path path, ServerCallContext context)
        {
            Console.WriteLine(path);
            return Task.FromResult(Program.Database.DeleteFile(path));
        }

        public override Task<ClientProto.StatusResponse> MoveFile(ClientProto.DoublePath path, ServerCallContext context)
        {
            Console.WriteLine(path);
            return Task.FromResult(Program.Database.MoveFile(path));
        }

        public override Task<ClientProto.BlockMessage> ReadFile(ClientProto.Path path, ServerCallContext context)
        {
            return Task.FromResult(Program.Database.ReadFile(path));
        }

        /* Below are original part */
        public override Task<ClientProto.StatusResponse> DeleteDirectory(ClientProto.Path path, ServerCallContext context)
        {
            return Task.FromResult(Program.Database.DeleteDirectory(path));
        }

        public override Task<ClientProto.ListOfNodesList> ListNodes(ClientProto.Path path, ServerCallContext context)
        {
            return Task.FromResult(Program.Database.ListDataNodesStoringReplicas(path));
        }

        public override Task<ClientProto.ListOfContents> ListContents(ClientProto.Path path, ServerCallContext context)
        {
            return Task.FromResult(Program.Database.ListDirectoryContents(path));
        }

        public override Task<ClientProto.BlockInfo> QueryBlockDestination(ClientProto.BlockInfo blockInfo, ServerCallContext context)
        {
            return Task.FromResult(Program.Database.AddBlockToFile(blockInfo));
        }
    }
}
