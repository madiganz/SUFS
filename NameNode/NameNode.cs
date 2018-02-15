using System;
namespace DataNode
{
    public class NameNode
    {
        public NameNode()
        {
        }
            private DirectoryInfo fileDirectory;
            //needs file directory

            public void processHeartbeat(){}

            public void processBlockReport(){}

            public void processClientStore(){}

            public void processClientRead(){}

        public void processClientDelete(){}

        public void CreateDirectory(){}
        public void DeleteDirectory(){}
        public void ListDirectoryContents(){}
        public void ListDataNodesStoringReplicas(){}

            public void redistribute(){}

            public void checkIfRedistributeNeeded(){}

    }
}
