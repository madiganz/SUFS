using System;
using System.Collections.Generic;
namespace NameNode.FileSystem
{
    public class File
    {
        public List<Guid> data { get; set; }
        public DateTime modifiedTime { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public int fileSize { get; set; }
        public File()
        {
            data = new List<Guid>();
            modifiedTime = DateTime.UtcNow;
        }
        public File(string name, string path)
        {
            this.name = name;
            this.path = path + "/" + name;
            data = new List<Guid>();
            modifiedTime = DateTime.UtcNow;
        }

    }
}
