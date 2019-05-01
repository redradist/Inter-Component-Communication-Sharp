using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ICCSharp.Networking
{
    public class TcpServer : Component, ITcpServer
    {
        private readonly LinkedList<TcpClient> _tcpClients = new LinkedList<TcpClient>();
        private readonly ThreadPool? _threadPool;
        
        private TcpListener? _listener;
        private volatile bool _accept;

        public event Action<TcpClient> ClientConnected;
        
        public TcpServer()
        {
        }
        
        public TcpServer(Component parent)
        : base(parent)
        {
        }
        
        public TcpServer(Component parent, int clientThreads)
        : base(parent)
        {
            _threadPool = new ThreadPool(clientThreads);
        }
        
        public TcpServer(int clientThreads)
        {
            _threadPool = new ThreadPool(clientThreads);
        }
        
        public void StartServer(string ipAddress, string port)
        {
            StartServer(IPAddress.Parse(ipAddress), int.Parse(port));
        }

        public void StartServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
            _listener.Start();
            _accept = true;

            StartTask(function: async () =>
            {
                Console.WriteLine($"Server started. Listening to TCP clients at tcp://127.0.0.1:{port}");
                await ListenAsync();
            });
            if (_isFactoryOwner)
            {
                Run();
            }
        }
        
        public void StopServer()
        {
            _accept = false;
            _listener?.Stop();
        }

        private async Task ListenAsync()
        {
            if (_listener != null)
            {
                // Continue listening ...  
                while (_accept)
                {
                    Console.WriteLine("Waiting for client...");
                    var client = await _listener.AcceptTcpClientAsync(); // Get the client
                    if (client != null)
                    {
                        Console.WriteLine("Client connected. Waiting for data.");
                        var customClient = new TcpClient(client);
                        StartClient(customClient);
                        _tcpClients.AddLast(customClient);
                        OnClientConnected(customClient);
                    }
                }
            }
        }

        private void StartClient(TcpClient client)
        {
            if (_threadPool != null)
            {
                _threadPool.StartTask(async () =>
                {
                    await client.ReadAsync();
                });
            }
            else
            {
                StartTask(async () =>
                {
                    await client.ReadAsync();
                });
            }
        }

        protected virtual void OnClientConnected(TcpClient obj)
        {
            ClientConnected?.Invoke(obj);
        }
    }
}