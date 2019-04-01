using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ICCSharp
{
    public class TcpClient : ITcpClient
    {
        private const int BufferSize = 4096;
        
        protected readonly System.Net.Sockets.TcpClient Client;

        public event Action<byte[]> DataReceived;
        
        public TcpClient(System.Net.Sockets.TcpClient client)
        {
            Client = client;
        }

        public virtual async Task Run()
        {
            using (NetworkStream stream = Client.GetStream())
            {
                int offset = 0;
                while (true)
                {
                    byte[] buffer = new byte[BufferSize];
                    await stream.ReadAsync(buffer, offset, buffer.Length);
                    HandleBuffer(buffer, out offset);
                }
            }
        }

        public virtual void HandleBuffer(byte[] buffer, out int offset)
        {
            OnDataReceived(buffer);
            offset = 0;
        }

        protected virtual void OnDataReceived(byte[] obj)
        {
            DataReceived?.Invoke(obj);
        }
    }
}