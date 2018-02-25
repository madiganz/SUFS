using Amazon.S3.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class StreamHolder : IDisposable
    {
        public List<GetObjectResponse> Responses { get; set; }
        public List<Stream> Streams { get; set; }
        public List<MemoryStream> MemStreams { get; set; }

        public StreamHolder()
        {
            Responses = new List<GetObjectResponse>();
            Streams = new List<Stream>();
            MemStreams = new List<MemoryStream>();
        }
        public void Dispose()
        {
            Streams.ForEach(s => s.Dispose());
            Responses.ForEach(r => r.Dispose());
        }
    }
}
