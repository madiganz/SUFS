using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataNode
{
    public static class Util
    {
        /// <summary>
        /// Gets blockID from list of metadata
        /// </summary>
        /// <param name="metaData">List of metadata</param>
        /// <returns>BlockID</returns>
        public static Guid GetBlockID(List<Metadata.Entry> metaData)
        {
            Guid id = new Guid();
            try
            {
                Metadata.Entry blckId = metaData.Find(m => { return m.Key == "blockid"; });
                id = Guid.Parse(blckId.Value);
                return id;
            }
            catch
            {
                return id;
            }
        }

        /// <summary>
        /// Gets block size from list of metadata
        /// </summary>
        /// <param name="metaData">List of metadata</param>
        /// <returns>Size of block</returns>
        public static int GetBlockSize(List<Metadata.Entry> metaData)
        {
            int blockSize = 0;
            try
            {
                Metadata.Entry size = metaData.Find(m => { return m.Key == "blocksize"; });
                blockSize = Convert.ToInt32(size.Value);
                return blockSize;
            }
            catch
            {
                return blockSize;
            }
        }
    }
}
