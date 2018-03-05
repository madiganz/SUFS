using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NameNode
{
    class DataNodeManager
    {
        private static List<DataNode> NodeList;
        private static int CurrentNodeIndex;

        private static DataNodeManager instance;

        /// <summary>
        /// Initialize the list of DataNodes
        /// </summary>
        private DataNodeManager()
        {
            NodeList = new List<DataNode>();
            CurrentNodeIndex = 0;
        }

        /// <summary>
        /// Singleton pattern to force only 1 instance per NameNode
        /// </summary>
        public static DataNodeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataNodeManager();
                }
                return instance;
            }
        }

        public void UpdateDataNodes(DataNodeProto.DataNodeInfo nodeInfo)
        {
            // Update list of datanodes
            int index = NodeList.FindIndex(node => node.ipAddress == nodeInfo.DataNode.IpAddress);

            // Not found -> add to list
            if (index < 0)
            {
                DataNode node = new DataNode(nodeInfo.DataNode.IpAddress, nodeInfo.DiskSpace, DateTime.UtcNow);
                Program.nodeList.Add(node);
            }
            else // Found, update lastHeartBeat timestamp
            {
                Program.nodeList[index].diskSpace = nodeInfo.DiskSpace;
                Program.nodeList[index].lastHeartBeat = DateTime.UtcNow;
            }
#if DEBUG
            Console.WriteLine("DataNodes:");
            foreach(var node in NodeList)
            {
                Console.WriteLine(node.ipAddress + " " + node.lastHeartBeat);
            }
            Console.WriteLine();
#endif
        }

        /// <summary>
        /// Simply return a list of the next 3 DataNodes' ip addresses in list
        /// </summary>
        /// <returns>List of the next 3 DataNodes in list</returns>
        public List<string> GetDataNodesForReplication()
        {
            if(NodeList.Count == 0)
            {
                return null;
            }
            List<string> ipAddresses = new List<string>
            {
                NodeList[CurrentNodeIndex++].ipAddress,
                NodeList[CurrentNodeIndex++ % NodeList.Count].ipAddress,
                NodeList[CurrentNodeIndex++ % NodeList.Count].ipAddress
            };
            CurrentNodeIndex %= NodeList.Count;
            return ipAddresses;
        }

        public async Task RunNodeCheck(CancellationToken token = default(CancellationToken))
        {
            while (!token.IsCancellationRequested)
            {
                CheckDeadNodes();
                try
                {
                    await Task.Delay(60000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private void CheckDeadNodes()
        {
            Console.WriteLine("Checking dead nodes");
            foreach (var node in NodeList)
            {
                TimeSpan span = DateTime.UtcNow.Subtract(node.lastHeartBeat);
                // Too much time has passed
                if (span.Minutes >= 10)
                {
                    NodeList.Remove(node);
                }

            }
        }
    }
}
