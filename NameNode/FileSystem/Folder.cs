using System;
using System.Collections.Generic;
namespace NameNode.FileSystem
{
    public class Folder 
    {
        public Dictionary<string, Folder> subfolders { get; set; }
        public Dictionary<string, File> files { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public Folder()
        {
            subfolders = new Dictionary<string, Folder>(); 
            files = new Dictionary<string, File>(); 
        }
        public Folder(string name, string path)
        {
            subfolders = new Dictionary<string, Folder>(); 
            files = new Dictionary<string, File>();
            this.path = path + "/" + name;
            this.name = name;
        }

        public bool FileExists(string file)
        {
            return files.ContainsKey(file);
        }
    }
}
