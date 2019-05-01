using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ICCSharp.Networking
{
    public interface ITcpClient
    {
        event Action<byte[]> DataReceived;
        Task ConnectAsync(IPAddress parse, int port);
        Task SendAsync(byte[] buffer);
        Task ReadAsync();
    }
}