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

        // Initial directory to store blocks in
        private static DirectoryInfo baseDirectoryInfo;

        /// <summary>
        /// Initialize the block storage
        /// </summary>
        private BlockStorage()
        {
            blockStorageMap = new Dictionary<Guid, string>();
            baseDirectoryInfo = Directory.CreateDirectory("data");
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
            string sFilePath = null;
            blockStorageMap.TryGetValue(blockUUID, out sFilePath);

            // Path found for uuid
            if(sFilePath != null)
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
        /// <param name="data">block data</param>
        /// <returns>Boolean indicating operator success</returns>
        public bool WriteBlock(Guid blockUUID, byte[] data)
        {
            try
            {
                string filePath = Guid.NewGuid().ToString();
                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                // Write to the file using StreamWriter class 
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.Write(data);
                sw.Flush();

                // Add to dictionary
                blockStorageMap.Add(blockUUID, filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes block data from disk
        /// </summary>
        /// <param name="blockUUID">Unique identifier of block</param>
        /// <returns>Boolean indicating operator success</returns>
        public bool DeleteBlock(Guid blockUUID)
        {
            string sFilePath = null;
            blockStorageMap.TryGetValue(blockUUID, out sFilePath);

            // Path found for uuid
            if (sFilePath != null)
            {
                try
                {
                    // Delete file
                    File.Delete(sFilePath);

                    // Remove from dictionary
                    blockStorageMap.Remove(blockUUID);
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
        /// Chooses a directory to store block data
        /// </summary>
        /// <returns>Full path of file</returns>
        private string ChooseRandomDirectory()
        {
            return TraverseDirectoryStructure(baseDirectoryInfo.ToString());
        }

        /// <summary>
        /// Creates a random directory
        /// </summary>
        /// <returns>Full path of directory</returns>
        private static string CreateRandomDirectory()
        {
            Guid guid = Guid.NewGuid();
            string dir = guid.ToString();
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Walks through the root directory structure. Creates file if there are less than 40 files.
        /// Creates sub directory if there are less than 20 sub directories.
        /// </summary>
        /// <param name="root"></param>
        /// <returns>Directory path that meets criteria</returns>
        private static string TraverseDirectoryStructure(string root)
        {
            // Data structure to hold names of subfolders to be
            // examined for files.
            Stack<string> dirs = new Stack<string>(40);

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
                    return root.ToString();
                }
                else if (subDirs.Length < 20) // Just create a directory
                {
                    return CreateRandomDirectory();
                }
                else
                {
                    // Push the subdirectories onto the stack for traversal.
                    // This could also be done before handing the files.
                    foreach (string str in subDirs)
                    {
                        dirs.Push(str);
                    }
                    continue;
                }
            }

            // If for some reason root does not exist, we will create it
            baseDirectoryInfo = Directory.CreateDirectory("data");
            return baseDirectoryInfo.ToString();
        }
    }
}
