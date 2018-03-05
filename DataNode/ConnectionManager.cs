using Grpc.Core;
using System;
using System.Collections.Generic;

namespace DataNode
{
    class ConnectionManager
    {
        private static ConnectionManager instance;

        // Channel for NameNode
        public Channel NameNodeConnection { get; set; }

        // List of channels for each block id
        private static Dictionary<Guid,Channel> channelMap;

        /// <summary>
        /// Initialize the channel pool
        /// </summary>
        private ConnectionManager()
        {
            channelMap = new Dictionary<Guid, Channel>();
        }

        /// <summary>
        /// Singleton pattern to force only 1 instance per datanode
        /// </summary>
        public static ConnectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnectionManager();
                }
                return instance;
            }
        }

        //public Channel GetChannel(string ipAddress, string port)
        public Channel GetChannel(Guid blockId)
        {
            //return channelMap.Find(c => c.Target == ipAddress + ":" + port);
            channelMap.TryGetValue(blockId, out Channel channel);
            return channel;
        }

        public Channel CreateChannel(Guid blockId, string ipAddress, string port)
        {
            Channel channel = new Channel(ipAddress + ":" + port, ChannelCredentials.Insecure);
            //Console.WriteLine(channel.State.ToString());
            channelMap.Add(blockId, channel);
            return channel;
        }

        public void ShutDownChannel(Guid blockId, Channel channel)
        {
            channel.ShutdownAsync().Wait();
            channelMap.Remove(blockId);
        }
    }
}
