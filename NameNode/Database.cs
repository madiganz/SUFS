using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NameNode.FileSystem;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NameNode
{

    public class Database
    {
        public Database()
        {
            InitializeFileDirectory();

        }

        private static Folder Root = null;
        private Folder CurrentDirectory;
        private static Dictionary<Guid, List<string>> BlockID_To_ip;

        //needs file directory

        public bool FileExists(string path)
        {
            try
            {
                string name = TraverseFileSystem(path);
                return (CurrentDirectory.subfolders.ContainsKey(name) || CurrentDirectory.files.ContainsKey(name));
            }
            catch
            {
                return false;
            }
        }


        public ClientProto.StatusResponse CreateFile(ClientProto.Path fullPath)
        {
            // Wrap in try block, if fails then returns false
            try
            {
                // If a file exists, fail the execution
                if (FileExists(fullPath.FullPath))
                {
                    return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.FileExists };
                }

                // Creates the file
                FileSystem.File file = new FileSystem.File();
                file.name = TraverseFileSystem(fullPath.FullPath);
                file.path = fullPath.FullPath;

                // Saves to file system
                CurrentDirectory.files.Add(file.name, file);
                SaveFileDirectory();
                //It done did it
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Ok };
            }
            catch (Exception e)
            {
                // Logs the error to console
                Console.WriteLine(e.Message);
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            }
        }

        public ClientProto.BlockInfo AddBlockToFile(ClientProto.BlockInfo addedBlock)
        {
            try
            {
                string path = addedBlock.FullPath;
                string name = TraverseFileSystem(path);
                FileSystem.File file = CurrentDirectory.files[name];
                file.fileSize += addedBlock.BlockSize;

                // to be used to send back a write request to the client
                List<string> responseRequests = new List<string>();

                // stores blockID and adds it to the file.
                Guid id = Guid.Parse(addedBlock.BlockId.Value);
                if(!file.data.Contains(id))
                    file.data.Add(id);

                List<string> ipAddresses = DataNodeManager.Instance.GetDataNodesForReplication();

                // add it to the BlockID to DataNode Dictionary
                BlockID_To_ip.TryAdd(id, new List<string>());
                SaveFileDirectory();
                return new ClientProto.BlockInfo { BlockId = new ClientProto.UUID { Value = id.ToString() }, FullPath = path, BlockSize = addedBlock.BlockSize, IpAddress = { ipAddresses } };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public ClientProto.BlockMessage ReadFile(ClientProto.Path wrappedPath)
        {
            try
            {
                string path = wrappedPath.FullPath;
                // Breaks up the path variable into the path's segments
                string name = TraverseFileSystem(path);

                // Grabs the parent directory
                FileSystem.File toBeRead = CurrentDirectory.files[name];

                List<ClientProto.BlockInfo> blockInfos = new List<ClientProto.BlockInfo>();

                List<string> ipAddresses;

                //checks block ids based on the file requested
                foreach (Guid blockID in toBeRead.data)
                {
                    //cross references those against the list of ids to ips
                    //choose ips for each block of the file
                    ipAddresses = BlockID_To_ip[blockID];

                    blockInfos.Add(new ClientProto.BlockInfo {
                        BlockId = new ClientProto.UUID { Value = blockID.ToString() },
                        BlockSize = Constants.MaxBlockSize,
                        IpAddress = { ipAddresses } });
                }

                //send back to client the ips and what to search for on the datanode
                return new ClientProto.BlockMessage {
                    BlockInfo = { blockInfos } ,
                    FileSize = toBeRead.fileSize,
                    Type = ClientProto.BlockMessage.Types.StatusType.Success
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ClientProto.BlockMessage {
                    BlockInfo = { new List<ClientProto.BlockInfo>() },
                    FileSize = 0,
                    Type = ClientProto.BlockMessage.Types.StatusType.Fail };
            }
        }

        public ClientProto.StatusResponse DeleteFile(ClientProto.Path wrappedPath)
        {
            try
            {
                string path = wrappedPath.FullPath;
                string name = TraverseFileSystem(path);
                FileSystem.File toBeDeleted = CurrentDirectory.files[name];
                //queue up requests for each of the datanodes that have blocks
                //      to delete as soon as they send in heartbeat/block report
                foreach (Guid blockID in toBeDeleted.data)
                {
                    foreach (string ipAddress in BlockID_To_ip[blockID])
                    {
                        DataNodeManager.Instance.AddRequestToNode(ipAddress, new DataNodeProto.BlockCommand
                        {
                            Action = DataNodeProto.BlockCommand.Types.Action.Delete,
                            BlockList = new DataNodeProto.BlockList
                            {
                                BlockId = { new DataNodeProto.UUID { Value = blockID.ToString() } }
                            }
                        });
                    }
                    BlockID_To_ip.Remove(blockID);
                }
                //remove file from directory system
                CurrentDirectory.files.Remove(name);
                SaveFileDirectory();
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            }
        }

        public ClientProto.StatusResponse CreateDirectory(ClientProto.Path wrappedPath)
        {
            try
            {
                string path = wrappedPath.FullPath;
                if (FileExists(path))
                    return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.FileExists };
                string name = TraverseFileSystem(path);
                Folder folder = new Folder(name, path);
                CurrentDirectory.subfolders.Add(folder.name, folder);
                SaveFileDirectory();
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail };
            }
        }

        public ClientProto.StatusResponse DeleteDirectory(ClientProto.Path wrappedPath)
        {
            string message = "";
            try
            {
                string path = wrappedPath.FullPath;
                if (path != "root")
                {
                    string name = TraverseFileSystem(path);
                    Folder parentFolder = CurrentDirectory;
                    Folder folderToDelete = CurrentDirectory.subfolders[name];

                    //loop call to subdirectories
                    while(folderToDelete.subfolders.Count > 0)
                    {
                        string key = folderToDelete.subfolders.Keys.First();
                        DeleteDirectory(new ClientProto.Path { FullPath = folderToDelete.subfolders[key].path });
                    }

                    //loop call delete files
                    while(folderToDelete.files.Count > 0)
                    {
                        string key = folderToDelete.files.Keys.First();
                        DeleteFile(new ClientProto.Path { FullPath = folderToDelete.files[key].path });
                    }

                    //delete directory
                    parentFolder.subfolders.Remove(name);
                    SaveFileDirectory();
                    return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };
                }
                else
                {
                    message = "Cannot delete Root.";
                    Console.WriteLine(message);
                    return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail, Message = "Cannot delete Root." };
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                Console.WriteLine(message);
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail, Message = message };
            }

        }

        public ClientProto.ListOfContents ListDirectoryContents(ClientProto.Path wrappedPath)
        {
            try
            {
                string path = wrappedPath.FullPath;
                //return a list of all subdirectories and files for current file
                string[] paths = ExtractPath(path);
                if (GrabDirectory(path) != null)
                {
                    List<string> returnList = new List<string>();
                    Folder directory = CurrentDirectory;
                    foreach (Folder folder in directory.subfolders.Values)
                    {
                        returnList.Add(folder.name + "/");
                    }

                    foreach (FileSystem.File file in directory.files.Values)
                    {
                        returnList.Add(file.name);
                    }

                    return new ClientProto.ListOfContents { Type = ClientProto.ListOfContents.Types.StatusType.Success, FileName = { returnList } };
                }
                else
                {
                    Console.WriteLine("Folder does not exist.");
                    return new ClientProto.ListOfContents { Type = ClientProto.ListOfContents.Types.StatusType.DirectoryDoesNotExist,
                        FileName = { new List<string>() } };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ClientProto.ListOfContents
                {
                    Type = ClientProto.ListOfContents.Types.StatusType.DirectoryDoesNotExist,
                    FileName = { new List<string>() }
                }; ;
            }
        }

        public ClientProto.ListOfNodesList ListDataNodesStoringReplicas(ClientProto.Path wrappedPath)
        {
            try
            {
                string path = wrappedPath.FullPath;
                //cross reference file name to block ids and locations
                string name = TraverseFileSystem(path);

                List<string> responseLocations = new List<string>();
                FileSystem.File requestedFile = CurrentDirectory.files[name];

                List<ClientProto.ListOfNodes> responseList = new List<ClientProto.ListOfNodes>();


                //queue up requests for each of the datanodes that have blocks
                //      to delete as soon as they send in heartbeat/block report
                foreach (Guid blockID in requestedFile.data)
                {
                    List<string> ips = new List<string>();
                    foreach (string ipAddress in BlockID_To_ip[blockID])
                    {
                        ips.Add(ipAddress);
                    }
                    responseList.Add(new ClientProto.ListOfNodes { BlockId = blockID.ToString(), NodeId = { ips } });
                }
                return new ClientProto.ListOfNodesList { ListOfNodes = { responseList } , Type = ClientProto.ListOfNodesList.Types.StatusType.Success };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ClientProto.ListOfNodesList { ListOfNodes = { new List<ClientProto.ListOfNodes>() } ,
                    Type = ClientProto.ListOfNodesList.Types.StatusType.FileDoesNotExist, Message = "Specified File Not Found."};
            }
        }

        public ClientProto.StatusResponse MoveFile(ClientProto.DoublePath doublePath)
        {
            try
            {
                string path = doublePath.Fullpath;
                string newPath = doublePath.Newpath;
                string oldName = TraverseFileSystem(path);
                Folder oldFolder = CurrentDirectory;

                string newName = TraverseFileSystem(newPath);

                FileSystem.File toBeMoved = oldFolder.files[oldName];

                oldFolder.files.Remove(oldName);
                toBeMoved.name = newName;
                toBeMoved.path = $"{CurrentDirectory.path}/{newName}";
                CurrentDirectory.files.Add(newName, toBeMoved);
                SaveFileDirectory();
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Success };

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ClientProto.StatusResponse { Type = ClientProto.StatusResponse.Types.StatusType.Fail, Message = "Internal Server Failure" };
            }

        }

        public List<string> GetIPsFromBlock(Guid blockId)
        {
            if(!BlockID_To_ip.ContainsKey(blockId))
            {
                return new List<string>{"Delete it"};
            }
            return BlockID_To_ip[blockId];
        }

        public void RemoveIPToBlockReferences(string ipAddress){

            foreach (List<string> ipAddresses in BlockID_To_ip.Values)
            {
                if (ipAddresses.Contains(ipAddress))
                {
                    ipAddresses.Remove(ipAddress);
                }
            }
        }

        private void SaveFileDirectory()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Root);
            System.IO.File.WriteAllText($"{Directory.GetCurrentDirectory()}/data/fileSystem.yml", yaml);

#if DEBUG
            Console.WriteLine(yaml);
#endif
        }

        //Loads the directory from file.
        private void InitializeFileDirectory()
        {
            if (Root == null)
            {
                try
                {
                    string yaml = System.IO.File.ReadAllText($"{Directory.GetCurrentDirectory()}/data/fileSystem.yml");
                    var input = new StringReader(yaml);

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new CamelCaseNamingConvention())
                        .Build();

                    Root = deserializer.Deserialize<Folder>(input);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Filesystem does not exist");
                    Console.Error.WriteLine("Creating new filesystem . . .");
                    Directory.CreateDirectory("data");
                    Root = new Folder();
                }
                BlockID_To_ip = new Dictionary<Guid, List<string>>();
            }
            CurrentDirectory = Root;
        }

        private string[] ExtractPath(string path)
        {
            return path.Split('/');
        }

        // returns the name of the file
        private string TraverseFileSystem(string path)
        {
            string[] paths = ExtractPath(path);
            GrabParentDirectory(path);
            return paths[paths.Length - 1];
        }

        private Folder GrabDirectory(string path)
        {
            string[] paths = ExtractPath(path);
            CurrentDirectory = Root;
            for (int i = 0; i < paths.Length; i++)
            {
                if(!CurrentDirectory.subfolders.ContainsKey(paths[i]))
                {
                    return null;
                }
                CurrentDirectory = CurrentDirectory.subfolders[paths[i]];
            }
            return CurrentDirectory;
        }

        private Folder GrabParentDirectory(string path)
        {
            string[] paths = ExtractPath(path);
            string currentPath = "";
            bool firstFile = true;
            Folder currentFolder;
            CurrentDirectory = Root;
            for (int i = 0; i < paths.Length - 1; i++)
            {
                if (firstFile)
                {
                    currentPath += paths[i];
                    firstFile = false;
                }
                else
                    currentPath += "/" + paths[i];
                if (!CurrentDirectory.subfolders.ContainsKey(paths[i]))
                {
                    currentFolder = CurrentDirectory;
                    CreateDirectory(new ClientProto.Path { FullPath = currentPath });
                    CurrentDirectory = currentFolder;
                }
                CurrentDirectory = CurrentDirectory.subfolders[paths[i]];
            }

            return CurrentDirectory;
        }

        private string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public Dictionary<Guid,List<string>> GrabBlockToIpDictionary()
        {
            return BlockID_To_ip;
        }

    }
}
