using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataNodeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initialize blockstorage
            BlockStorage mBlockStorage = BlockStorage.Instance;
            //Task heartBeatTask = SendHeartBeat();
            //heartBeatTask.Wait();
            Thread heartBeatThread = new Thread(new ThreadStart(HeartBeat.SendHeartBeat));
            Thread blockReportThread = new Thread(new ThreadStart(BlockReport.SendBlockReport));
            heartBeatThread.Start();
            blockReportThread.Start();
            //SendHeartBeat();
            while (true)
            {
            }
        }

        //public async static Task SendHeartBeat()
        //{
        //    while (true)
        //    {
        //        Console.WriteLine("I AM A HEARTBEAT");
        //        await Task.Delay(2000);
        //    }
        //}

        //static void SendHeartBeat()
        //{
        //    Task.Run(async () =>
        //    {
        //        while (true)
        //        {
        //            Console.WriteLine("I AM A HEARTBEAT");
        //            await Task.Delay(2000);
        //        }
        //    });
        //}
    }
}
