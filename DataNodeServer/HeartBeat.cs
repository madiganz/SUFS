using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNodeServer
{
    public static class HeartBeat
    {
        public static void SendHeartBeat()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine("I AM A HEARTBEAT");
                    await Task.Delay(3000); // This is an HDFS default
                }
            });
        }
    }
}
