using Grpc.Core;
using System;
using System.Threading;

namespace Client
{
	class Program
	{
		public static void Main(string[] args)
		{
            //// Assume passed in parameters:
            //// NameNodeIP:Port => args[0],
			Channel channel = new Channel(args[0], ChannelCredentials.Insecure);

			var client = new ClientProto.ClientProto.ClientProtoClient(channel);

			var reply = client.DeleteDirectory( new ClientProto.Path { Fullpath = "Path" } );

			Console.WriteLine("Greeting: " + reply);

		
			channel.ShutdownAsync().Wait();

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}
	}
}

