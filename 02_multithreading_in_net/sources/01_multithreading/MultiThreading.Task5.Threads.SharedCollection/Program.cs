/*
 * 5. Write a program which creates two threads and a shared collection:
 * the first one should add 10 elements into the collection and the second should print all elements
 * in the collection after each adding.
 * Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MultiThreading.Task5.Threads.SharedCollection;

static class Program
{
    static void Main()
    {
        Console.WriteLine("5. Write a program which creates two threads and a shared collection:");
        Console.WriteLine("the first one should add 10 elements into the collection and the second should print all elements in the collection after each adding.");
        Console.WriteLine("Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.");
        Console.WriteLine();

        Console.WriteLine("Producer adds 1..10, consumer prints growing snapshots.");

        var buffer = new BlockingCollection<int>();
        var consumer = new Thread(() =>
        {
            var snapshot = new List<int>();
            foreach (var item in buffer.GetConsumingEnumerable())
            {
                snapshot.Add(item);
                Console.WriteLine("[" + string.Join(", ", snapshot) + "]");
            }
        });

        var producer = new Thread(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                buffer.Add(i);
            }

            buffer.CompleteAdding();
        });

        consumer.Start();
        producer.Start();

        producer.Join();
        consumer.Join();

        Console.ReadLine();
    }
}