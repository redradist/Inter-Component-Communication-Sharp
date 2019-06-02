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
    }
}