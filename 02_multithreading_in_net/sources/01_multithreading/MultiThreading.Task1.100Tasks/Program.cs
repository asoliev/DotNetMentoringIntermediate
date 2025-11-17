/*
 * 1.	Write a program, which creates an array of 100 Tasks, runs them and waits all of them are not finished.
 * Each Task should iterate from 1 to 1000 and print into the console the following string:
 * “Task #0 – {iteration number}”.
 */
using System;
using System.Threading.Tasks;

namespace MultiThreading.Task1._100Tasks;

static class Program
{
    const int TaskAmount = 100;
    const int MaxIterationsCount = 1000;

    static void Main()
    {
        Console.WriteLine(".Net Mentoring Program. Multi threading V1.");
        Console.WriteLine("1. Write a program, which creates an array of 100 Tasks, runs them and waits all of them are not finished.");
        Console.WriteLine("Each Task should iterate from 1 to 1000 and print into the console the following string:");
        Console.WriteLine("“Task #0 – {iteration number}”.");
        Console.WriteLine();
        
        HundredTasks();

        Console.ReadLine();
    }

    static void HundredTasks()
    {
        var tasks = new Task[TaskAmount];

        for (int i = 0; i < tasks.Length; i++)
        {
            int taskNum = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 1; j <= MaxIterationsCount; j++)
                    Output(taskNum, j);
            });
        }

        try
        {
            Task.WaitAll(tasks);
        }
        catch (AggregateException agg)
        {
            foreach (var ex in agg.Flatten().InnerExceptions)
            {
                Console.WriteLine($"Task error: {ex.GetType().Name}: {ex.Message}");
            }
        }

        Console.WriteLine("All tasks are finished (or failed).");
    }

    static void Output(int taskNumber, int iterationNumber) =>
        Console.WriteLine($"Task #{taskNumber} – {iterationNumber}");
}