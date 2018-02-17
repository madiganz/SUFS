using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    public static class HeartBeat
    {
        public static async void SendHeartBeat(object client)
        {
            //Task.Run(async () =>
            //{
            while (true)
            {
                DataNodeProto.HeartBeatRequest heartBeatRequest = new DataNodeProto.HeartBeatRequest { HeartBeatString = "I AM A HEARTBEAT" };
                DataNodeProto.HeartBeatResponse response = ((DataNodeProto.DataNodeService.DataNodeServiceClient)client).SendHeartBeat(heartBeatRequest);
                Console.WriteLine(response);
                //Console.WriteLine("I AM A HEARTBEAT");
                await Task.Delay(3000); // This is an HDFS default
            }
            //});
        }
    }
}
