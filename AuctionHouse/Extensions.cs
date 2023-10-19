using System;
using System.Net.Sockets;
using System.Text;

namespace AuctionHouse
{
	public static class Extensions
	{
        // these two methods are from in-class work
		public static string ReadString(this TcpClient client)
		{
            NetworkStream inStream = client.GetStream();
            byte[] array = new byte[client.ReceiveBufferSize];
            inStream.Read(array, 0, array.Length);
            string @string = Encoding.ASCII.GetString(array);
            return @string.Substring(0, @string.IndexOf("\0", StringComparison.Ordinal));
        }

        public static void WriteString(this TcpClient client, string response)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            NetworkStream outStream = client.GetStream();
            outStream.Write(bytes, 0, bytes.Length);
            outStream.Flush();
        }
    }
}

