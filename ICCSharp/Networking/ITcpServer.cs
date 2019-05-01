using System.Net;

namespace ICCSharp.Networking
{
    public interface ITcpServer
    {
        void StartServer(string ipAddress, string port);
        void StartServer(IPAddress ipAddress, int port);
        void StopServer();
    }
}