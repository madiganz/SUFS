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
        private static int RoundRobinDistributionIndex;

        private static DataNodeManager instance;

        /// <summary>
        /// Initialize the list of DataNodes
        /// </summary>
        private DataNodeManager()
        {
            NodeList = new List<DataNode>();
            RoundRobinDistributionIndex = 0;
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
            int index = FindNodeIndexFromIP(nodeInfo.DataNode.IpAddress);

            // Not found -> add to list
            if (index < 0)
            {
                DataNode node = new DataNode(nodeInfo.DataNode.IpAddress, nodeInfo.DiskSpace, DateTime.UtcNow);
                NodeList.Add(node);
            }
            else // Found, update lastHeartBeat timestamp
            {
                NodeList[index].DiskSpace = nodeInfo.DiskSpace;
                NodeList[index].LastHeartBeat = DateTime.UtcNow;
            }
#if DEBUG
            Console.WriteLine("DataNodes:");
            foreach(var node in NodeList)
            {
                Console.WriteLine(node.IpAddress + " " + node.LastHeartBeat);
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
            List<string> ipAddresses = new List<string>
            {
                GrabNextDataNode().IpAddress,
                GrabNextDataNode().IpAddress,
                GrabNextDataNode().IpAddress
            };
            return ipAddresses;
        }

        public List<DataNodeProto.BlockCommand> ProcessBlockReport(Guid[] blockList, string currentNodeIP)
        {
            try
            {
                List<string> currentBlock;
                List<DataNodeProto.BlockCommand> returnRequests = new List<DataNodeProto.BlockCommand>();
                int index = FindNodeIndexFromIP(currentNodeIP);
                DataNode currentDataNode = NodeList[index];

                //For each BlockID in report:
                foreach (Guid blockID in blockList)
                {
                    // Grab the list of ips that are connected to this BlockID
                    currentBlock = Program.Database.GetIPsFromBlock(blockID);
                    if (CheckIfRedistributeNeeded(currentBlock))
                        returnRequests.Add(Redistribute(currentBlock, currentNodeIP, blockID));

                }

                //Loops through to see if there are any more requests to send to this node
                foreach (DataNodeProto.BlockCommand request in currentDataNode.Requests)
                {
                    returnRequests.Add(request);
                }
                currentDataNode.Requests.Clear();


                //either send empty list back or send queued commands to the specific datanode
                return returnRequests;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public DataNode GrabNextDataNode()
        {
            if (RoundRobinDistributionIndex % NodeList.Count == 0)
                RoundRobinDistributionIndex = 0;
            return NodeList[RoundRobinDistributionIndex++ % NodeList.Count].IpAddress;

        }

        public DataNodeProto.BlockCommand Redistribute(List<string> currentBlock, string currentNodeIP, Guid blockID)
        {
            string ipAddress = NodeList[RoundRobinDistributionIndex++ % NodeList.Count].IpAddress;
            // While it isn't a node that contains this block, choose the next in the node list
            while (ipAddress == currentNodeIP || currentBlock.Contains(ipAddress))
            {
                ipAddress = GrabNextDataNode().IpAddress;
            }

            List<DataNodeProto.DataNode> nodes = new List<DataNodeProto.DataNode>
            {
                new DataNodeProto.DataNode {
                    IpAddress = ipAddress
                }
            };

            DataNodeProto.DataBlock dataBlock = new DataNodeProto.DataBlock
            {
                BlockId = new DataNodeProto.UUID { Value = blockID.ToString() },
                DataNodes = { nodes }
            };

            // Tell the node where to send a copy of the current block
            DataNodeProto.BlockCommand blockCommand = new DataNodeProto.BlockCommand
            {
                Action = DataNodeProto.BlockCommand.Types.Action.Transfer,
                DataBlock = { dataBlock }
            };

            return blockCommand;
        }

        public bool CheckIfRedistributeNeeded(List<string> currentBlock)
        {
            // If the Block is not above the minimum ReplicationFactor
            return currentBlock.Count < Constants.ReplicationFactor;
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
                TimeSpan span = DateTime.UtcNow.Subtract(node.LastHeartBeat);
                // Too much time has passed
                if (span.Minutes >= 10)
                {
                    NodeList.Remove(node);
                }

            }
        }

        public void AddRequestToNode(string ipAddress, DataNodeProto.BlockCommand blockCommand)
        {
            int index = FindNodeIndexFromIP(ipAddress);
            NodeList[index].Requests.Add(blockCommand);
        }

        public int FindNodeIndexFromIP(string ipAddress)
        {
            return NodeList.FindIndex(node => node.IpAddress == ipAddress);
        }
    }
}
