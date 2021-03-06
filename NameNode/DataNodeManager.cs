﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NameNode
{
    /// <summary>
    /// Class to manage the data nodes
    /// </summary>
    class DataNodeManager
    {
        /// <summary>
        /// List of DataNodes that are connected to the NameNode
        /// </summary>
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

        /// <summary>
        /// Updates the list of Datanodes
        /// </summary>
        /// <param name="nodeInfo">A single DataNodes info</param>
        public DataNodeProto.BlockCommand UpdateDataNodes(DataNodeProto.DataNodeInfo nodeInfo)
        {
            // Update list of datanodes
            int index = FindNodeIndexFromIP(nodeInfo.DataNode.IpAddress);
            // Not found -> add to list
            if (index < 0)
            {
                DataNode node = new DataNode(nodeInfo.DataNode.IpAddress, nodeInfo.DiskSpace, DateTime.UtcNow);
                NodeList.Add(node);
                index = FindNodeIndexFromIP(nodeInfo.DataNode.IpAddress);
                if (NodeList.Count <= Constants.ReplicationFactor)
                {
                    BelowReplicationFactorNewDataNodeRedistribute(node.IpAddress);
                }
                else
                {
                    CheckIfRedistributeNeeded();
                }
            }
            else // Found, update lastHeartBeat timestamp
            {
                NodeList[index].DiskSpace = nodeInfo.DiskSpace;
                NodeList[index].LastHeartBeat = DateTime.UtcNow;
            }
            DataNodeProto.BlockCommand returnCommand;
            if (NodeList[index].Requests.Count > 0){
                returnCommand = NodeList[index].Requests[0];
                NodeList[index].Requests.Remove(returnCommand);
            }else
                returnCommand = new DataNodeProto.BlockCommand();
            
            return returnCommand;
        }

        /// <summary>
        /// Simply return a list of the next 3 DataNodes' ip addresses in list
        /// </summary>
        /// <returns>List of the next 3 DataNodes in list</returns>
        public List<string> GetDataNodesForReplication()
        {
            List<string> ipAddresses = new List<string>();
            int factor = Math.Min(NodeList.Count, Constants.ReplicationFactor);
            for (int i = 0; i < factor; i++)
                ipAddresses.Add(GrabNextDataNode().IpAddress);
            return ipAddresses;
        }

        /// <summary>
        /// Processes the blockreport. Need to add requests when a datanode has died causing replications to fall before replication factor
        /// </summary>
        /// <param name="blockList">Blocks contained in DataNode</param>
        /// <param name="currentNodeIP">DataNode's IP Address</param>
        /// <returns>List of Commands to be sent back to the DataNode</returns>
        public DataNodeProto.StatusResponse ProcessBlockReport(Guid[] blockList, string currentNodeIP)
        {
            try
            {
                List<string> currentBlock;
                //DataNodeProto.StatusResponse returnRequests = new DataNodeProto.BlockCommand();
                int index = FindNodeIndexFromIP(currentNodeIP);
                DataNode currentDataNode = NodeList[index];

                Program.Database.RemoveIPToBlockReferences(currentNodeIP);

                //For each BlockID in report:
                foreach (Guid blockID in blockList)
                {
                    // Grab the list of ips that are connected to this BlockID
                    currentBlock = Program.Database.GetIPsFromBlock(blockID);
                    if(currentBlock.Contains("Delete it"))
                    {
                        AddRequestToNode(currentNodeIP, new DataNodeProto.BlockCommand
                        {
                            Action = DataNodeProto.BlockCommand.Types.Action.Delete,
                            BlockList = new DataNodeProto.BlockList
                            {
                                BlockId = { new DataNodeProto.UUID { Value = blockID.ToString() } }
                            }
                        });
                    }else{
                        currentBlock.Add(currentNodeIP);
                    }   

                }


                //either send empty list back or send queued commands to the specific datanode
                return new DataNodeProto.StatusResponse { Type = DataNodeProto.StatusResponse.Types.StatusType.Success };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new DataNodeProto.StatusResponse { Type = DataNodeProto.StatusResponse.Types.StatusType.Fail };
            }
        }

        /// <summary>
        /// Gets next DataNode and moves index
        /// </summary>
        /// <returns>DataNode</returns>
        public DataNode GrabNextDataNode()
        {
            if (RoundRobinDistributionIndex % NodeList.Count == 0)
                RoundRobinDistributionIndex = 0;
            return NodeList[RoundRobinDistributionIndex++ % NodeList.Count];

        }

        /// <summary>
        /// Data block redistribution log. Chooses a DataNode to forward to that does not currently contain the block
        /// </summary>
        /// <param name="currentBlock">The data block infomation, which contains list of addresses the block is stored on</param>
        /// <param name="currentNodeIP">The DataNode's IP Address</param>
        /// <param name="blockID">Unique ID of block</param>
        /// <returns>Command for the DataNode to run</returns>
        public void Redistribute(List<string> currentBlock, string currentNodeIP, Guid blockID)
        {
            List<DataNodeProto.DataNode> nodes = new List<DataNodeProto.DataNode>();
            for (int i = 0; i < Constants.ReplicationFactor - currentBlock.Count; i++)
            {
                string ipAddress = GrabNextDataNode().IpAddress;
                // While it isn't a node that contains this block, choose the next in the node list
                while (ipAddress == currentNodeIP || currentBlock.Contains(ipAddress))
                {
                    ipAddress = GrabNextDataNode().IpAddress;
                }
                Console.WriteLine("Node to be added for redistribution: " + ipAddress + " for blockid = " + blockID.ToString());

                nodes.Add(
                    new DataNodeProto.DataNode
                    {
                        IpAddress = ipAddress
                    });
            }

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

            AddRequestToNode(currentNodeIP, blockCommand);
        }

        public void BelowReplicationFactorNewDataNodeRedistribute(string newDataNodeIP)
        {
            List<DataNodeProto.DataNode> destinationNode = new List<DataNodeProto.DataNode>
            {
                new DataNodeProto.DataNode
                {
                    IpAddress = newDataNodeIP
                }
            };

            Dictionary<string, List<DataNodeProto.DataBlock>> commands = new Dictionary<string, List<DataNodeProto.DataBlock>>();
            foreach (KeyValuePair<Guid, List<string>> entry in Program.Database.GrabBlockToIpDictionary())
            {
                if (!commands.ContainsKey(entry.Value[0]))
                {
                    commands.Add(entry.Value[0], new List<DataNodeProto.DataBlock>());
                }
                commands[entry.Value[0]].Add(
                    new DataNodeProto.DataBlock
                    {
                        BlockId = new DataNodeProto.UUID { Value = entry.Key.ToString() },
                        DataNodes = { destinationNode }
                    });
            }
            foreach (KeyValuePair<string, List<DataNodeProto.DataBlock>> entry in commands)
            {
                AddRequestToNode(entry.Key,
                    new DataNodeProto.BlockCommand
                    {
                        Action = DataNodeProto.BlockCommand.Types.Action.Transfer,
                        DataBlock = { entry.Value }
                    });
            }
        }

        /// <summary>
        /// Checks if block redistribution is needed based on replication factor.
        /// If needed, will queue redistribution.
        /// </summary>
        /// <returns>True if redistribution is needed</returns>
        public void CheckIfRedistributeNeeded()
        {
            // If the Block is not above the minimum ReplicationFactor
            foreach (KeyValuePair<Guid, List<string>> entry in Program.Database.GrabBlockToIpDictionary())
            {
                if (entry.Value.Count < Constants.ReplicationFactor)
                {
                    if (entry.Value.Count != 0)
                        Redistribute(entry.Value, entry.Value[0], entry.Key);
                }
            }
        }

        /// <summary>
        /// Run a node check every minute
        /// </summary>
        /// <param name="token">Cancellation token for task</param>
        /// <returns>Task</returns>
        public async Task RunNodeCheck()
        {
            while (true)
            {
                CheckDeadNodes();
                try
                {
                    await Task.Delay(30000);
                }
                catch (Exception)
                {
                    Console.WriteLine("Node check failed...");
                    break;
                }
            }
        }

        /// <summary>
        /// Checks whether or not the Node is dead. If it is, it removes it from the list.
        /// </summary>
        private void CheckDeadNodes()
        {
            Console.WriteLine("Checking for dead DataNodes...");
            for(var i = 0; i < NodeList.Count; i++)
            {
                TimeSpan span = DateTime.UtcNow.Subtract(NodeList[i].LastHeartBeat);
                if (span.Minutes >= 1)
                {
                    Console.WriteLine("DataNode died: " + NodeList[i].IpAddress);
                    Program.Database.RemoveIPToBlockReferences(NodeList[i].IpAddress);
                    NodeList.RemoveAt(i);
                }
            }
            if (NodeList.Count >= Constants.ReplicationFactor)
            {
                CheckIfRedistributeNeeded();
            }
        }

        /// <summary>
        /// Adds a request that will be sent to a DataNode to perform
        /// </summary>
        /// <param name="ipAddress">Address of DataNode</param>
        /// <param name="blockCommand">Command to be added</param>
        public void AddRequestToNode(string ipAddress, DataNodeProto.BlockCommand blockCommand)
        {
            int index = FindNodeIndexFromIP(ipAddress);
            NodeList[index].Requests.Add(blockCommand);
        }

        /// <summary>
        /// Simple helper function to return a Node's index given an ip address
        /// </summary>
        /// <param name="ipAddress">IP Address of DataNode</param>
        /// <returns>Index DataNode in list</returns>
        public int FindNodeIndexFromIP(string ipAddress)
        {
            return NodeList.FindIndex(node => node.IpAddress == ipAddress);
        }

        /// <summary>
        /// Clears the list of requests to be sent to DataNode
        /// </summary>
        /// <param name="ipAddress">IP Address of DataNode</param>
        public void ClearRequests(string ipAddress)
        {
            var index = FindNodeIndexFromIP(ipAddress);
            NodeList[index].Requests.Clear();
        }
    }
}
