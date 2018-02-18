using Grpc.Core;
using System;
using System.Threading;

namespace Client
{
	class Program
	{
		public static void Main(string[] args)
		{
			Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

			var client = new ClientProto.ClientProto.ClientProtoClient(channel);

			//String user = "Tong";

			var reply = client.DeleteDirectory( new ClientProto.Path { Fullpath = "Path" } );

			Console.WriteLine("Greeting: " + reply);

		
			channel.ShutdownAsync().Wait();

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}
	}
}

