using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ICCSharp.Networking
{
    public class TcpClient : Component, ITcpClient
    {
        private const int BufferSize = 4096;
        
        protected readonly System.Net.Sockets.TcpClient Client;

        public event Action<byte[]> DataReceived;
        
        public TcpClient()
        {
            Client = new System.Net.Sockets.TcpClient();
        }
        
        public TcpClient(System.Net.Sockets.TcpClient client)
        {
            Client = client;
        }
        
        public TcpClient(Component parent)
            : base(parent)
        {
            Client = new System.Net.Sockets.TcpClient();
        }

        public TcpClient(Component parent, System.Net.Sockets.TcpClient client)
            : base(parent)
        {
            Client = client;
        }
        
        public virtual async Task ConnectAsync(IPAddress parse, int port)
        {
            await Client.ConnectAsync(parse, port);
        }

        public virtual async Task SendAsync(byte[] buffer)
        {
            using (NetworkStream stream = Client.GetStream())
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        
        public virtual async Task ReadAsync()
        {
            using (NetworkStream stream = Client.GetStream())
            {
                while (true)
                {
                    byte[] buffer = new byte[BufferSize];
                    int readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (readBytes == 0)
                    {
                        // End of stream. Finish reading data
                        break;
                    }
                    HandleBuffer(buffer, readBytes);
                }
            }
        }
        
        protected virtual void HandleBuffer(byte[] buffer, int readBytes)
        {
            OnDataReceived(buffer);
        }

        protected virtual void OnDataReceived(byte[] obj)
        {
            DataReceived?.Invoke(obj);
        }
    }
}