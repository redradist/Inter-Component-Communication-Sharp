using System.Threading.Tasks;

namespace ICCSharp
{
    public interface ITcpClient
    {
        Task Run();
        void HandleBuffer(byte[] buffer, out int offset);
    }
}