using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    static class Constants
    {
        public const int Port = 50051;
        public const int BlockReportInterval = 300000; // 5 minutes
        public const int HeartBeatInterval = 3000; // Default hdfs
        public const int BlockSize = 134217728; // 128MB Default hdfs
        public const int StreamChunkSize = 2097152; // 2MB Default size for GRPC streaming
    }
}