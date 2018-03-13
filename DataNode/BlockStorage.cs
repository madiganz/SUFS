using System;
using System.Collections.Generic;
using System.IO;

namespace DataNode
{
    class BlockStorage
    {
        private static BlockStorage instance;

        // Structure to keep track of block data
        private static Dictionary<Guid, string> blockStorageMap;

        private static string root;

        /// <summary>
        /// Initialize the block storage
        /// </summary>
        private BlockStorage()
        {
            blockStorageMap = new Dictionary<Guid, string>();

            // Use the current directory/data as the root directory
            Directory.CreateDirectory("data");
            root = Directory.GetCurrentDirectory() + "/data";

            // Looks at file system to add any files to memory
            AddFilesToMemory(root);
        }

        /// <summary>
        /// Singleton pattern to force only 1 instance per datanode
        /// </summary>
        public static BlockStorage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BlockStorage();
                }
                return instance;
            }
        }

        /// <summary>
        /// Returns an array of all block ids
        /// </summary>
        public static Guid[] GetBlocks()
        {
            Dictionary<Guid, string>.KeyCollection keys = blockStorageMap.Keys;
            Guid[] blockList = new Guid[keys.Count];
            if (keys.Count > 0)
            {
                keys.CopyTo(blockList, 0);
            }
            return blockList;
        }

        /// <summary>
        /// Read block data from local disk
        /// </summary>
        /// <param name="blockUUID">Unique identifier of block</param>
        /// <returns>block data. null if not found</returns>
        public byte[] ReadBlock(Guid blockUUID)
        {
            byte[] blockData = null;
            blockStorageMap.TryGetValue(blockUUID, out string sFilePath);

            // Path found for uuid
            if (sFilePath != null)
            {
                FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Read);
                try
                {
                    int length = (int)fs.Length;  // get file length
                    blockData = new byte[length]; // create buffer
                    int count;                    // actual number of bytes read
                    int sum = 0;                  // total number of bytes read

                    // read until Read method returns 0 (end of the stream has been reached)
                    while ((count = fs.Read(blockData, sum, length - sum)) > 0)
                        sum += count;  // sum is a buffer offset for next reading
                }
                finally
                {
                    fs.Close();
                }
            }

            return blockData;
        }

        /// <summary>
        /// Writes block data to local disk.
        /// </summary>
        /// <param name="blockUUID">Unique identifier of block</param>
        /// <param path="path">Full path of file</param>
        /// <param name="data">block data</param>
        /// <returns>Boolean indicating operator success</returns>
        public bool WriteBlock(Guid blockUUID, string path, byte[] data)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(data, 0, data.Length);
                }

                //// Add to dictionary
                blockStorageMap.Add(blockUUID, path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a file given a blockid
        /// </summary>
        /// <param name="blockUUID">Unique identifier of block</param>
        /// <returns>Full path of created file</returns>
        public string CreateFile(Guid blockUUID)
        {
            //string filePath = ChooseRandomDirectory();

            // Making it simple and just create files in root
            string filePath = root + "/" + blockUUID.ToString() + "."; // Extensionless file
            blockStorageMap.Add(blockUUID, filePath);
            return filePath;
        }

        /// <summary>
        /// Deletes block data from disk
        /// </summary>
        /// <param name="blockUUID">Unique identifier of block</param>
        /// <returns>Boolean indicating operator success</returns>
        public bool DeleteBlock(Guid blockUUID)
        {
            // Path found for uuid
            if (blockStorageMap.TryGetValue(blockUUID, out string sFilePath))
            {
                try
                {
                    // Delete file
                    File.Delete(sFilePath);

                    // Remove from dictionary
                    blockStorageMap.Remove(blockUUID);
                    Console.WriteLine("Deleted block: " + blockUUID.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // No file to be deleted
            return true;
        }

        /// <summary>
        /// Validates that the block has the correct size
        /// </summary>
        /// <param name="blockUUID">Unique identifier of block</param>
        /// <param name="filePath">Full path of file</param>
        /// <returns>True is block size is correct, otherwise false</returns>
        public bool ValidateBlock(Guid blockUUID, string filePath, int blockSize)
        {
            try
            {
                FileInfo info = new FileInfo(filePath);
                bool valid = info.Length == blockSize;

                if (!valid)
                {
                    DeleteBlock(blockUUID);
                }
                return valid;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Chooses a directory to store block data
        /// </summary>
        /// <returns>Full path of file</returns>
        private string ChooseRandomDirectory()
        {
            return TraverseDirectoryStructure(root);
        }

        /// <summary>
        /// Creates a random directory
        /// </summary>
        /// <returns>Full path of directory</returns>
        private static string CreateRandomDirectory(string path)
        {
            Guid guid = Guid.NewGuid();
            string dir = guid.ToString();
            Directory.CreateDirectory(path + @"\" + dir);
            return dir;
        }

        /// <summary>
        /// Walks through the root directory structure. Creates file if there are less than 40 files.
        /// Creates sub directory if there are less than 20 sub directories.
        /// </summary>
        /// <param name="root">Root directory</param>
        /// <returns>Directory path that meets criteria</returns>
        private static string TraverseDirectoryStructure(string root)
        {
            // Data structure to hold names of subfolders to be
            // examined for files.
            Stack<string> dirs = new Stack<string>();

            // Root should always exist
            try
            {
                dirs.Push(root);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e);
            }

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                string[] files = null;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {

                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                // Just create a file
                if (files.Length < 40)
                {
                    return currentDir;
                }
                else if (subDirs.Length < 20) // Just create a directory
                {
                    return CreateRandomDirectory(currentDir);
                }
                else
                {
                    // Push the subdirectories onto the stack for traversal.
                    foreach (string str in subDirs)
                    {
                        dirs.Push(str);
                    }
                    continue;
                }
            }

            return root;
        }

        private void AddFilesToMemory(string root)
        {
            string[] paths = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
            foreach(var p in paths)
            {
                blockStorageMap.Add(Guid.Parse(System.IO.Path.GetFileNameWithoutExtension(p)), p);
            }
        }

        public long GetFreeDiskSpace()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                // Only care about current drive
                if (drive.Name == "/dev")
                {
                    return drive.AvailableFreeSpace;
                }
            }

            return 0;
        }
    }
}
