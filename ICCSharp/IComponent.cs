using System.Threading.Tasks;

namespace ICCSharp
{
    public interface IComponent
    {
        TaskFactory GetTaskFactory();
        void Run();
        void Run(bool isRunInThread);
        void Stop();
        void Join();
    }
}