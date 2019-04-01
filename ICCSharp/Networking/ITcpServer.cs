using System.Net;

namespace ICCSharp
{
    public interface ITcpServer
    {
        void StartServer(string ipAddress, string port);
        void StartServer(IPAddress ipAddress, int port);
        void StopServer();
    }
}