using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ICCSharp
{
    public class TcpServer : Component
    {
        private readonly LinkedList<object> _tcpClients = new LinkedList<object>();
        private TcpListener Listener { get; set; }
        private bool Accept { get; set; } = false;
        private readonly ThreadPool _threadPool;
        private readonly Type _clientType;

        public event Action<object> ClientConnected;
        
        public TcpServer(Type clientType)
        {
            ValidateClientType(clientType);
            _clientType = clientType;
        }

        public TcpServer(Type clientType, int clientThreads)
        {
            ValidateClientType(clientType);
            _clientType = clientType;
            _threadPool = new ThreadPool(clientThreads);
        }
        
        private void ValidateClientType(Type clientType)
        {
            if (clientType.GetInterface("ICCSharp.ITcpClient") == null)
            {
                throw new NotSupportedException($"clientType should implement ICC.ITcpClient interface !!"); 
            }
        }
        
        public void StartServer(string ipAddress, string port)
        {
            StartServer(IPAddress.Parse(ipAddress), int.Parse(port));
        }

        public void StartServer(IPAddress ipAddress, int port)
        {
            Listener = new TcpListener(ipAddress, port);
            Listener.Start();
            Accept = true;

            StartTask(function: async () =>
            {
                Console.WriteLine($"Server started. Listening to TCP clients at tcp://127.0.0.1:{port}");
                await ListenAsync();
            });
            Run();
        }
        
        public void StopServer()
        {
            Accept = false;
            Listener.Stop();
        }

        private async Task ListenAsync()
        {
            if (Listener != null && Accept)
            {
                // Continue listening.  
                while (true)
                {
                    Console.WriteLine("Waiting for client...");
                    var client = await Listener.AcceptTcpClientAsync(); // Get the client
                    if (client != null)
                    {
                        Console.WriteLine("Client connected. Waiting for data.");
                        var customClient = _clientType
                                           .GetConstructor(types: new[] {client.GetType()})
                                           .Invoke(parameters: new object[] {client});
                        _tcpClients.AddLast(customClient);
                        OnClientConnected(customClient);
                        StartClient(client);
                    }
                }
            }
        }

        private void StartClient(object client)
        {
            if (_threadPool != null)
            {
                _threadPool.StartTask(async () =>
                {
                    await (Task) client
                                 .GetType()
                                 .GetMethod("Run")
                                 .Invoke(client, parameters: new object[] { client });
                });
            }
            else
            {
                StartTask(async () =>
                {
                    await (Task) client
                                 .GetType()
                                 .GetMethod("Run")
                                 .Invoke(client, parameters: new object[] { client });
                });
            }
        }

        protected virtual void OnClientConnected(object obj)
        {
            ClientConnected?.Invoke(obj);
        }
    }
}