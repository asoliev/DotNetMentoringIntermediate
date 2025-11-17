/*
 * 4.	Write a program which recursively creates 10 threads.
 * Each thread should be with the same body and receive a state with integer number, decrement it,
 * print and pass as a state into the newly created thread.
 * Use Thread class for this task and Join for waiting threads.
 * 
 * Implement all of the following options:
 * - a) Use Thread class for this task and Join for waiting threads.
 * - b) ThreadPool class for this task and Semaphore for waiting threads.
 */

using System;
using System.Threading;

namespace MultiThreading.Task4.Threads.Join;

static class Program
{
    static void Main()
    {
        Console.WriteLine("4. Write a program which recursively creates 10 threads.");
        Console.WriteLine("Each thread should be with the same body and receive a state with integer number, decrement it, print and pass as a state into the newly created thread.");
        Console.WriteLine("Implement all of the following options:");
        Console.WriteLine();
        Console.WriteLine("- a) Use Thread class for this task and Join for waiting threads.");
        Console.WriteLine("- b) ThreadPool class for this task and Semaphore for waiting threads.");
        Console.WriteLine();

        Console.WriteLine("Option (a) - Thread + Join:");
        RunWithThreads(initialValue: 10);

        Console.WriteLine();
        Console.WriteLine("Option (b) - ThreadPool + Semaphore:");
        RunWithThreadPool(initialValue: 10);

        Console.WriteLine();
        Console.WriteLine("Done. Press Enter to exit.");
        Console.ReadLine();
    }

    // Option (a): use Thread and Join.
    // Each thread decrements the integer state, prints it, then creates the next thread and waits (Join) for it.
    static void RunWithThreads(int initialValue)
    {
        // Root thread that starts the recursive creation.
        var root = new Thread(ThreadProc);
        root.Start(initialValue);
        root.Join();
    }

    static void ThreadProc(object? state)
    {
        if (state is not int value) return;

        // Decrement and print
        value--;
        Console.WriteLine($"[Thread:{Environment.CurrentManagedThreadId}] value = {value}");

        // If there are more to create, start a new thread passing the decremented value and join it.
        if (value > 0)
        {
            var child = new Thread(ThreadProc);
            child.Start(value);
            child.Join(); // wait for the child to finish before this thread exits
        }
    }

    // Option (b): use ThreadPool and SemaphoreSlim for waiting threads.
    // We'll use a SemaphoreSlim with initial count 0 and release it once per finished work item.
    // Main thread will wait for exactly `initialValue` releases.
    static void RunWithThreadPool(int initialValue)
    {
        using SemaphoreSlim semaphore = new(0, initialValue);

        // Work callback for ThreadPool threads.
        void PoolCallback(object? state)
        {
            if (state is not int value) return;

            // Decrement and print
            value--;
            Console.WriteLine($"[ThreadPool:{Environment.CurrentManagedThreadId}] value = {value}");

            // If more work is needed, queue the next work item with the decremented value.
            if (value > 0)
                ThreadPool.QueueUserWorkItem(PoolCallback, value);

            // Signal completion of this work item.
            semaphore.Release();
        }

        // Queue the first work item and then wait for `initialValue` completions.
        ThreadPool.QueueUserWorkItem(PoolCallback, initialValue);

        // Wait for all created work items to signal completion.
        for (int i = 0; i < initialValue; i++)
            semaphore.Wait();
    }
}