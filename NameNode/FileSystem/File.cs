using System;
using System.Collections.Generic;
namespace NameNode.FileSystem
{
    public class File
    {
        public List<Guid> data { get; set; }
        public DateTime modifiedTime { get; set; }
        public string name { get; set; }
        public int fileSize { get; set; }
        public File()
        {
            data = new List<Guid>();
            modifiedTime = DateTime.UtcNow;
        }
        public File(string name)
        {
            this.name = name;
            data = new List<Guid>();
            modifiedTime = DateTime.UtcNow;
        }
    }
}
