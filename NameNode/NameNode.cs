using System;
using System.IO;
using NameNode.FileSystem;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NameNode
{
    
    public class NameNode
    {
        public NameNode()
        {
            initializeFileDirectory();
        }
        private DirectoryInfo fileDirectory;
        private Folder Root;
        private Folder CurrentDirectory;

        //needs file directory

        public void processHeartbeat()
        {
            //either send back "OK"
            //or send queued commands to the specific datanode
        }

        public void processBlockReport()
        {
            //For each block id in report:
                //check to see if the datanode to id exists
                    // if not, add it
                //either send OK back or send queued commands to the specific datanode
        }

        public void CreateFile()
        {
            FileSystem.File file = new FileSystem.File();
            file.name = "";
            file.fileSize = 0;
            for (int i = 0; i < file.fileSize % Constants.MaxBlockSize; i++)
            {
                Guid id = Guid.NewGuid();
                file.data.Add(id);
                redistribute(id.ToString());
            }
            CurrentDirectory.files.Add(file.name, file);

        }

        public void ReadFile()
        {
            //checks block ids based on the file requested
            //cross references those against the list of ids to ips
            //choose ips for each block of the file
            //send back to client the ips and what to search for on the datanode
        }

        public void DeleteFile()
        {
            //queue up requests for each of the datanodes that have blocks
            //      to delete as soon as they send in heartbeat/block report
            //remove file from directory system
        }

        public void CreateDirectory()
        {
            Folder folder = new Folder();
            folder.name = "";
            folder.parent = CurrentDirectory;
            CurrentDirectory.subfolders.Add(folder.name, folder);
        }

        public void DeleteDirectory()
        {
            //recursive call to subdirectories
            //loop call delete files
            //IF AT ROOT STOP HERE
            //move current directory up a layer
            //delete directory
        }

        public void MoveDownDirectory()
        {
            if(CurrentDirectory.subfolders.ContainsKey(""))
                CurrentDirectory = CurrentDirectory.subfolders[""];
        }

        public void MoveUpDirectory()
        {
            if (CurrentDirectory != Root)
                CurrentDirectory = CurrentDirectory.parent;
        }

        public void ListDirectoryContents()
        {
            //return a list of all subdirectories and files for current file
        }

        public void ListDataNodesStoringReplicas()
        {
            //cross reference file name to block ids and locations
        }

        public void redistribute(string blockID)
        {
            //takes blockID and finds what ips it is on
            //queues a task to be sent to one of the acive ips to:
                //duplicate the block on a different ip
            //returns
        }

        public void checkIfRedistributeNeeded(string blockID)
        {
            //checks if blockID is a valid one
            //counts to see if number of blocks are >= replication factor
                //if <, call redistribute
            //return
        }

        private void initializeFileDirectory()
        {
            try
            {
                string yaml = System.IO.File.ReadAllText(@"/Users/BryanHerr/Documents/College/CPSC4910-02/SUFS/testDirecotry.yml");
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
            CurrentDirectory = Root;
        }

    }
}
