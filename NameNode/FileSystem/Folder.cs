using System;
using System.Collections.Generic;
namespace NameNode.FileSystem
{
    public class Folder 
    {
        public Dictionary<string, Folder> subfolders { get; set; }
        public Dictionary<string, File> files { get; set; }
        public Folder parent { get; set; }
        public string name { get; set; }
        public Folder()
        {
            subfolders = new Dictionary<string, Folder>(); 
            files = new Dictionary<string, File>(); 
        }
        public Folder(string name, Folder parent)
        {
            subfolders = new Dictionary<string, Folder>(); 
            files = new Dictionary<string, File>(); 
            this.name = name;
            this.parent = parent;
        }
    }
}
