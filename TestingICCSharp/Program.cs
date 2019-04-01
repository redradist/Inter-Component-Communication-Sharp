using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ICCSharp;

namespace TestingICCSharp
{
    class Program
    {
        static async Task DoSomething()
        {
            Console.WriteLine($"CurrentThreadId is {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1000);
            Console.WriteLine($"CurrentThreadId is {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1000);
            Console.WriteLine($"CurrentThreadId is {Thread.CurrentThread.ManagedThreadId}");
        }
        
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer(typeof(TcpClient));
            server.StartServer("127.0.0.1", "49000");
//            Component parent = new Component();
//            Console.WriteLine("Start task");
//            Task task = parent.StartTask(function: async () =>
//            {
//                Stopwatch sw = new Stopwatch();
//                sw.Start();
//                await DoSomething();
//                sw.Stop();
//                Console.WriteLine($"Elapsed={sw.Elapsed}");
//                parent.Stop();
//            });
//            parent.Run();
//            var thread = new Thread(() =>
//            {
//                parent.Run();
//            });
//            thread.Start();
//            thread.Join();
            Console.WriteLine("Finish awaiting task !!");
        }
    }
}