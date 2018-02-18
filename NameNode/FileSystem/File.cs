using System;
using System.Collections.Generic;
namespace NameNode.FileSystem
{
    public class File
    {
        public Dictionary<Guid, BlockNode> data { get; set; }
        public DateTime modifiedTime { get; set; }
        public string name { get; set; }
        public File()
        {
            data = new Dictionary<Guid, BlockNode>();
            modifiedTime = DateTime.UtcNow;
        }
        public File(string name)
        {
            this.name = name;
            data = new Dictionary<Guid, BlockNode>();
            modifiedTime = DateTime.UtcNow;
        }
    }
}
