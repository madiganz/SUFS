using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameNode
{
    class DataNode
    {
        public string ipAddress { get; set; }
        public long diskSpace { get; set; }
        public DateTime lastHeartBeat { get; set; }
        public List<string> requests { get; set; }

        public DataNode( string ip, long space, DateTime time)
        {
            ipAddress = ip;
            diskSpace = space;
            lastHeartBeat = time;
            requests = new List<string>();
        }
    }
}
