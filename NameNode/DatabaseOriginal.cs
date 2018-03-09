//using System;
//using System.Collections.Generic;
//using System.IO;
//using NameNode.FileSystem;
//using YamlDotNet.Serialization;
//using YamlDotNet.Serialization.NamingConventions;

//namespace NameNode
//{
    
//    public class Database
//    {
//        public Database()
//        {
//            initializeFileDirectory();
//        }
//        private DirectoryInfo fileDirectory;
//        private Folder Root;
//        private Folder CurrentDirectory;

//        //needs file directory

//        public void processHeartbeat()
//        {
//            //either send back "OK"
//            //or send queued commands to the specific datanode
//        }

//        public void processBlockReport()
//        {
//            //For each block id in report:
//                //check to see if the datanode to id exists
//                    // if not, add it
//                //either send OK back or send queued commands to the specific datanode
//        }

//        public bool CreateFile(string path)
//        {
//            try
//            {
//                string[] paths = ExtractPath(path);
//                GrabParentDirectory(path);
//                FileSystem.File file = new FileSystem.File();
//                file.name = paths[paths.Length-1];
//                file.fileSize = 0;
//                for (int i = 0; i < file.fileSize % Constants.MaxBlockSize; i++)
//                {
//                    Guid id = Guid.NewGuid();
//                    file.data.Add(id);
//                    redistribute(id.ToString());
//                }
//                CurrentDirectory.files.Add(file.name, file);
//                SaveFileDirectory();
//            }
//            catch(Exception e)
//            {
//                Console.WriteLine(e);
//                return false;
//            }
//            return true;

//        }

//        public List<string> ReadFile(string path)
//        {
//            try
//            {
//                string[] paths = ExtractPath(path);
//                GrabParentDirectory(path);
//                List<string> ips = new List<string>();
//                //checks block ids based on the file requested
//                //cross references those against the list of ids to ips
//                //choose ips for each block of the file
//                //send back to client the ips and what to search for on the datanode
//                return ips;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                return null;
//            }

//        }

//        public bool RenameFile(string path)
//        {
//            return false;
//        }

//        public void DeleteFile(string path)
//        {
//            GrabParentDirectory(path);
//            Folder currentFolder = CurrentDirectory;
//            //queue up requests for each of the datanodes that have blocks
//            //      to delete as soon as they send in heartbeat/block report
//            //remove file from directory system
//        }

//        public bool CreateDirectory(string path)
//        {
//            try
//            {
//                string[] paths = ExtractPath(path);
//                GrabParentDirectory(path);
//                Folder folder = new Folder(paths[paths.Length - 1], path);
//                CurrentDirectory.subfolders.Add(folder.name, folder);
//                SaveFileDirectory();
//                return true;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                return false;
//            }
//        }

//        public bool DeleteDirectory(string path)
//        {
//            try
//            {
//                if (path != "root")
//                {
//                    string[] paths = ExtractPath(path);
//                    GrabParentDirectory(path);
//                    Folder parentFolder = CurrentDirectory;
//                    Folder folderToDelete = CurrentDirectory.subfolders[paths[paths.Length - 1]];

//                    //loop call to subdirectories
//                    foreach (string key in folderToDelete.subfolders.Keys)
//                    {
//                        DeleteDirectory(folderToDelete.subfolders[key].path);
//                        folderToDelete.subfolders.Remove(key);
//                    }

//                    //loop call delete files
//                    foreach (string key in folderToDelete.files.Keys)
//                    {
//                        DeleteFile(folderToDelete.files[key].path);
//                        folderToDelete.files.Remove(key);
//                    }

//                    //delete directory
//                    parentFolder.subfolders.Remove(paths[paths.Length - 1]);
//                    SaveFileDirectory();
//                    return true;
//                }
//                else
//                {
//                    Console.WriteLine("Cannot delete Root. Root has been emptied");
//                    return false;
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                return false;
//            }

//        }

//        public bool MoveDirectory(string path, string newPath)
//        {
//            return false;
//        }

//        public List<string> ListDirectoryContents(string path)
//        {
//            //return a list of all subdirectories and files for current file
//            string[] paths = ExtractPath(path);
//            GrabDirectory(path);

//            List<string> returnList = new List<string>();
//            Folder directory = CurrentDirectory;
//            foreach(Folder folder in directory.subfolders.Values)
//            {
//                returnList.Add(folder.path);
//            }

//            foreach (FileSystem.File file in directory.files.Values)
//            {
//                returnList.Add(file.path);
//            }

//            return returnList;
//        }

//        public void ListDataNodesStoringReplicas()
//        {
//            //cross reference file name to block ids and locations
//        }

//        public void redistribute(string blockID)
//        {
//            //takes blockID and finds what ips it is on
//            //queues a task to be sent to one of the acive ips to:
//                //duplicate the block on a different ip
//            //returns
//        }

//        public void checkIfRedistributeNeeded(string blockID)
//        {
//            //checks if blockID is a valid one
//            //counts to see if number of blocks are >= replication factor
//                //if <, call redistribute
//            //return
//        }

//        private void SaveFileDirectory()
//        {
//            var serializer = new SerializerBuilder().Build();
//            var yaml = serializer.Serialize(Root);
//            System.IO.File.WriteAllText(@"C:/data/testDirecotry.yml", yaml);
//            Console.WriteLine(yaml);
//        }

//        private void initializeFileDirectory()
//        {
//            try
//            {
//                string yaml = System.IO.File.ReadAllText(@"C:/data/testDirecotry.yml");
//                var input = new StringReader(yaml);

//                var deserializer = new DeserializerBuilder()
//                    .WithNamingConvention(new CamelCaseNamingConvention())
//                    .Build();

//                Root = deserializer.Deserialize<Folder>(input);
//            }
//            catch (Exception e)
//            {
//                Console.Error.WriteLine(e.ToString());
//                Console.Error.WriteLine("Creating new filesystem . . .");
//                Root = new Folder();
//            }
//            CurrentDirectory = Root;
//        }

//        private string[] ExtractPath(string path)
//        {
//            return path.Split('/');
//        }

//        private Folder GrabDirectory(string path)
//        {
//            string[] paths = ExtractPath(path);
//            CurrentDirectory = Root;
//            for (int i = 0; i < paths.Length; i++)
//            {
//                if (!CurrentDirectory.subfolders.ContainsKey(paths[i]))
//                    CurrentDirectory.subfolders.Add(paths[i], new Folder(paths[i], CurrentDirectory.path + "/" + paths[i]));
//                CurrentDirectory = CurrentDirectory.subfolders[paths[i]];
//            }
//            return CurrentDirectory;
//        }

//        private Folder GrabParentDirectory(string path)
//        {
//            string[] paths = ExtractPath(path);
//            CurrentDirectory = Root;
//            for (int i = 0; i < paths.Length - 1; i++)
//            {
//                if (!CurrentDirectory.subfolders.ContainsKey(paths[i]))
//                    CurrentDirectory.subfolders.Add(paths[i], new Folder(paths[i], CurrentDirectory.path + "/" + paths[i]));
//                CurrentDirectory = CurrentDirectory.subfolders[paths[i]];
//            }
//            return CurrentDirectory;
//        }

//    }
//}
