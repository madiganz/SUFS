using System;
using System.Collections.Generic;
namespace NameNode.FileSystem
{
    public class BlockNode
    {
        public Guid id { get; set; }
        public List<string> ipAdresses { get; set; }
        public int size { get; set; }
        public BlockNode() 
        {
            ipAdresses = new List<string>();
        }
        public BlockNode(Guid ID, int size, List<string> IPAdresses)
        {
            this.id = ID;
            this.size = size;
            this.ipAdresses = new List<string>();
            foreach(string ip in IPAdresses)
            {
                this.ipAdresses.Add(ip);
            }
        }
    }
}
