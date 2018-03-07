using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameNode
{
    class DataNode
    {
        public string IpAddress { get; set; }
        public long DiskSpace { get; set; }
        public DateTime LastHeartBeat { get; set; }
        public List<DataNodeProto.BlockCommand> Requests { get; set; }
        public List<Guid> BlockIDs { get; set; }

        public DataNode( string ip, long space, DateTime time)
        {
            IpAddress = ip;
            DiskSpace = space;
            LastHeartBeat = time;
            Requests = new List<DataNodeProto.BlockCommand>();
            BlockIDs = new List<Guid>();
        }
    }
}
