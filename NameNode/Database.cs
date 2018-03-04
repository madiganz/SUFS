using System;
using System.Collections.Generic;
using System.IO;
using NameNode.FileSystem;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NameNode
{
    
    public class Database
    {

        //NOTE: ALL RESPONSES ARE AS FOLLOWS IN FORMAT = "ipAddress:BlockID"
        //NOTE: FOR SENDING THE LOCATIONS FOR THE DATANODE T FORWARD TO, THE FOLLOWING FORMAT IS USED = "ipAddress:BlockID=ipAddress:BlockID=..."
        //NOTE: DELETION FOR DATANODES WILL BE IN THE FOLLOWING FORMAT = "Delete:BlockID"
        public Database()
        {
            initializeFileDirectory();
            
        }

        private static Folder Root = null;
        private Folder CurrentDirectory;
        private static Dictionary<Guid, List<string>> BlockID_To_ip;
        private static int RoundRobinDistributionIndex;

        //needs file directory


        //~~~~Sorted out in the DataNodeHandler~~~~
        //public List<string> processHeartbeat(string ipAddress)
        //{
        //    //either send back "OK"
        //    //or send queued commands to the specific datanode
        //}

        public List<string> processBlockReport(Guid[] blockList, string currentNodeIP)
        {
            try
            {
                List<string> currentBlock;
                List<string> returnRequests = new List<string>();
                int index = Program.nodeList.FindIndex(node => node.ipAddress == currentNodeIP);
                DataNode currentDataNode = Program.nodeList[index];

                //For each BlockID in report:
                foreach (Guid blockID in blockList)
                {
                    // Grab the list of ips that are connected to this BlockID
                    currentBlock = BlockID_To_ip[blockID];
                    if (checkIfRedistributeNeeded(currentBlock))
                        returnRequests.Add(redistribute(currentBlock, currentNodeIP, blockID));

                }

                //Loops through to see if there are any more requests to send to this node
                foreach(string request in currentDataNode.requests)
                {
                    returnRequests.Add(request);
                }
                currentDataNode.requests.Clear();


                //either send empty list back or send queued commands to the specific datanode
                return returnRequests;
            } catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

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


        public List<string> CreateFile(string path, int size)
        {
            // Wrap in try block, if fails then returns false
            try
            {
                // If a file exists, fail the execution
                if (FileExists(path))
                {
                    return null;
                }

                // Creates the file
                FileSystem.File file = new FileSystem.File();
                file.name = TraverseFileSystem(path);
                file.fileSize = size;

                // to be used to send back a write request to the client
                List<string> responseRequests = new List<string>();

                // Builds the blocks to be distributed to other DataNodes
                for (int i = 0; i < file.fileSize % Constants.MaxBlockSize; i++)
                {
                    // stores blockID and adds it to the file.
                    Guid id = Guid.NewGuid();
                    file.data.Add(id);

                    List<string> ipAddresses = new List<string>();

                    //chooses DataNodes to store this block
                    string firstDataNodeRequest = "";
                    for (int j = 0; j < Constants.ReplicationFactor; j++)
                    {
                        ipAddresses.Add(GrabNextDataNode());
                        if (j == 0)
                            firstDataNodeRequest = $"{ipAddresses[j]}:{id}";
                        else
                            firstDataNodeRequest += $"={ipAddresses[j]}:{id}";
                    }

                    responseRequests.Add(firstDataNodeRequest);

                    // add it to the BlockID to DataNode Dictionary
                    BlockID_To_ip.Add(id, ipAddresses);
                }

                // Saves to file system
                CurrentDirectory.files.Add(file.name, file);
                SaveFileDirectory();
                //It done did it
                return responseRequests;
            }
            catch(Exception e)
            {
                // Logs the error to console
                Console.WriteLine(e);
                return null;
            }



        }

        public List<string> ReadFile(string path)
        {
            try
            {
                // Breaks up the path variable into the path's segments
                string name = TraverseFileSystem(path);

                // Grabs the parent directory
                FileSystem.File toBeRead = CurrentDirectory.files[name];

                List<string> ipAddresses;
                List<string> requestResponse = new List<string>();

                //checks block ids based on the file requested
                foreach (Guid blockID in toBeRead.data)
                {
                    //cross references those against the list of ids to ips
                    //choose ips for each block of the file
                    ipAddresses = BlockID_To_ip[blockID];
                    requestResponse.Add($"{ipAddresses[0]}:{blockID}");
                }

                //send back to client the ips and what to search for on the datanode
                return requestResponse;
            }catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public bool DeleteFile(string path)
        {
            try
            {
                string name = TraverseFileSystem(path);

                FileSystem.File toBeDeleted = CurrentDirectory.files[name];

                //queue up requests for each of the datanodes that have blocks
                //      to delete as soon as they send in heartbeat/block report
                foreach (Guid blockID in toBeDeleted.data)
                {
                    foreach (string ipAddress in BlockID_To_ip[blockID])
                    {
                        int index = Program.nodeList.FindIndex(node => node.ipAddress == ipAddress);
                        Program.nodeList[index].requests.Add($"Delete:{blockID}");
                    }
                }
                //remove file from directory system
                CurrentDirectory.files.Remove(name);
                SaveFileDirectory();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                if (FileExists(path))
                    return false;
                string name = TraverseFileSystem(path);
                Folder folder = new Folder(name, path);
                CurrentDirectory.subfolders.Add(folder.name, folder);
                SaveFileDirectory();
                return true;
            }catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool DeleteDirectory(string path)
        {
            try
            {
                if (path != "root")
                {
                    string name = TraverseFileSystem(path);
                    Folder parentFolder = CurrentDirectory;
                    Folder folderToDelete = CurrentDirectory.subfolders[name];

                    //loop call to subdirectories
                    foreach (string key in folderToDelete.subfolders.Keys)
                    {
                        DeleteDirectory(folderToDelete.subfolders[key].path);
                        folderToDelete.subfolders.Remove(key);
                    }

                    //loop call delete files
                    foreach (string key in folderToDelete.files.Keys)
                    {
                        DeleteFile(folderToDelete.files[key].path);
                        folderToDelete.files.Remove(key);
                    }

                    //delete directory
                    parentFolder.subfolders.Remove(name);
                    SaveFileDirectory();
                    return true;
                }
                else
                {
                    Console.WriteLine("Cannot delete Root. Root has been emptied");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }

        public List<string> ListDirectoryContents(string path)
        {
            try
            {
                //return a list of all subdirectories and files for current file
                string[] paths = ExtractPath(path);
                GrabDirectory(path);

                List<string> returnList = new List<string>();
                Folder directory = CurrentDirectory;
                foreach (Folder folder in directory.subfolders.Values)
                {
                    returnList.Add(folder.path);
                }

                foreach (FileSystem.File file in directory.files.Values)
                {
                    returnList.Add(file.path);
                }

                return returnList;
            }catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public List<string> ListDataNodesStoringReplicas(string path)
        {
            try
            {
                //cross reference file name to block ids and locations
                string name = TraverseFileSystem(path);

                List<string> responseLocations = new List<string>();
                FileSystem.File requestedFile = CurrentDirectory.files[name];

                //queue up requests for each of the datanodes that have blocks
                //      to delete as soon as they send in heartbeat/block report
                foreach (Guid blockID in requestedFile.data)
                {
                    foreach (string ipAddress in BlockID_To_ip[blockID])
                    {
                        responseLocations.Add($"{ipAddress}:{blockID}");
                    }
                }
                return responseLocations;
            }catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        // Will be done through MoveFile
        //public bool RenameFile(string path, string newName)
        //{
        //    try
        //    {

        //    }catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return false;
        //    }
            
        //}


        public bool MoveFile(string path, string newPath)
        {
            try
            {
                string oldName = TraverseFileSystem(path);
                Folder oldFolder = CurrentDirectory;

                string newName = TraverseFileSystem(newPath);

                FileSystem.File toBeMoved = oldFolder.files[oldName];

                oldFolder.files.Remove(oldName);
                toBeMoved.name = newName;
                toBeMoved.path = $"{CurrentDirectory.path}/{newName}";
                CurrentDirectory.files.Add(newName, toBeMoved);
                SaveFileDirectory();
                return true;

            }catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
        }

        private string GrabNextDataNode()
        {
            if (RoundRobinDistributionIndex % Program.nodeList.Count == 0)
                RoundRobinDistributionIndex = 0;
            return Program.nodeList[RoundRobinDistributionIndex++ % Program.nodeList.Count].ipAddress;
            
        }

        public string redistribute(List<string> currentBlock, string currentNodeIP, Guid blockID)
        {
            string ipAddress = Program.nodeList[RoundRobinDistributionIndex++ % Program.nodeList.Count].ipAddress;
            // While it isn't a node that contains this block, choose the next in the node list
            while (ipAddress == currentNodeIP || currentBlock.Contains(ipAddress))
            {
                ipAddress = GrabNextDataNode();
            }

            // Tell the node where to send a copy of the current block
            return $"{ipAddress}:{blockID}";
        }

        public bool checkIfRedistributeNeeded(List<string> currentBlock)
        {
            // If the Block is not above the minimum ReplicationFactor
            return currentBlock.Count < Constants.ReplicationFactor;
        }

        private void SaveFileDirectory()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Root);
            System.IO.File.WriteAllText(@"C:/data/testDirecotry.yml", yaml);
            Console.WriteLine(yaml);
        }

        //Loads the directory from file.
        private void initializeFileDirectory()
        {
            if (Root != null)
            {
                try
                {
                    string yaml = System.IO.File.ReadAllText(@"C:/data/testDirecotry.yml");
                    var input = new StringReader(yaml);

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new CamelCaseNamingConvention())
                        .Build();

                    Root = deserializer.Deserialize<Folder>(input);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                    Console.Error.WriteLine("Creating new filesystem . . .");
                    Root = new Folder();
                }
                BlockID_To_ip = new Dictionary<Guid, List<string>>();
                RoundRobinDistributionIndex = 0;
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
                CurrentDirectory = CurrentDirectory.subfolders[paths[i]];
            }
            return CurrentDirectory;
        }

        private Folder GrabParentDirectory(string path)
        {
            string[] paths = ExtractPath(path);
            CurrentDirectory = Root;
            for (int i = 0; i < paths.Length - 1; i++)
            {
                CurrentDirectory = CurrentDirectory.subfolders[paths[i]];
            }
            return CurrentDirectory;
        }

        private string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

    }
}
