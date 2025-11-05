/*
*  Create a Task and attach continuations to it according to the following criteria:
   a. Continuation task should be executed regardless of the result of the parent task.
   b. Continuation task should be executed when the parent task finished without success.
   c. Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation
   d. Continuation task should be executed outside of the thread pool when the parent task would be cancelled
   Demonstrate the work of the each case with console utility.
*/
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading.Task6.Continuation;

static class Program
{
    static void Main()
    {
        Console.WriteLine("Create a Task and attach continuations to it according to the following criteria:");
        Console.WriteLine("a. Continuation task should be executed regardless of the result of the parent task.");
        Console.WriteLine("b. Continuation task should be executed when the parent task finished without success.");
        Console.WriteLine("c. Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation.");
        Console.WriteLine("d. Continuation task should be executed outside of the thread pool when the parent task would be cancelled.");
        Console.WriteLine("Demonstrate the work of the each case with console utility.");
        Console.WriteLine();


        Console.WriteLine("a. Continuation regardless of parent result:");
        CaseA();

        Console.WriteLine("\nb. Continuation when parent finished without success (Faulted or Canceled):");
        CaseB();

        Console.WriteLine("\nc. Continuation on parent fail, reusing parent thread:");
        CaseC();

        Console.WriteLine("\nd. Continuation outside thread pool when parent is cancelled:");
        CaseD();

        Console.WriteLine("\nPress any key to exit...");


        Console.ReadLine();
    }

    // a. Continuation regardless of the result of the parent task.
    static void CaseA()
    {
        var parent = Task.Run(() =>
        {
            Console.WriteLine("  Parent running...");
            // Simulate work
            Thread.Sleep(500);
            // Uncomment to test failure: throw new Exception("Parent failed!");
        });

        parent.ContinueWith(t =>
        {
            Console.WriteLine("  Continuation runs regardless of parent result. Status: " + t.Status);
        }, TaskContinuationOptions.None).Wait();
    }

    // b. Continuation when the parent task finished without success (Faulted or Canceled)
    static void CaseB()
    {
        var parent = Task.Run(() =>
        {
            Console.WriteLine("  Parent running...");
            Thread.Sleep(500);
            throw new Exception("Parent failed!");
        });

        parent.ContinueWith(t =>
        {
            Console.WriteLine("  Continuation runs only if parent did NOT succeed. Status: " + t.Status);
        }, TaskContinuationOptions.NotOnRanToCompletion).Wait();
    }

    // c. Continuation when the parent task failed, reusing parent thread
    static void CaseC()
    {
        var parent = Task.Factory.StartNew(() =>
        {
            Console.WriteLine("  Parent running...");
            Thread.Sleep(500);
            throw new Exception("Parent failed!");
        });

        parent.ContinueWith(t =>
        {
            Console.WriteLine("  Continuation runs on parent fail, reusing parent thread. Status: " + t.Status);
            Console.WriteLine("  Is ThreadPool thread: " + Thread.CurrentThread.IsThreadPoolThread);
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Wait();
    }

    // d. Continuation outside of the thread pool when the parent task is cancelled
    static void CaseD()
    {
        var cts = new CancellationTokenSource();
        var parent = Task.Factory.StartNew(() =>
        {
            Console.WriteLine("  Parent running...");
            Thread.Sleep(500);
            cts.Token.ThrowIfCancellationRequested();
        }, cts.Token);

        // Cancel the parent task
        cts.Cancel();

        parent.ContinueWith(t =>
        {
            Console.WriteLine("  Continuation runs outside thread pool on parent cancel. Status: " + t.Status);
            Console.WriteLine("  Is ThreadPool thread: " + Thread.CurrentThread.IsThreadPoolThread);
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.LongRunning, TaskScheduler.Default).Wait();
    }
}