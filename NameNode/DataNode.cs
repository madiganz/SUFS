using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameNode
{
    class DataNode
    {
        public Guid nodeId { get; set; }
        public string ipAddress { get; set; }
        public long diskSpace { get; set; }
        public DateTime lastHeartBeat { get; set; }

        public DataNode(string id, string ip, long space, DateTime time)
        {
            nodeId = Guid.Parse(id);
            ipAddress = ip;
            diskSpace = space;
            lastHeartBeat = time;
        }
    }
}
