﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNode
{
    static class Constants
    {
        public const int Port = 50051;
        public const int BlockReportInterval = 21600000; // Default hdfs
        public const int HeartBeatInterval = 3000; // Default hdfs
        public const int BlockSize = 134217728; // 128MB Default hdfs
    }
}