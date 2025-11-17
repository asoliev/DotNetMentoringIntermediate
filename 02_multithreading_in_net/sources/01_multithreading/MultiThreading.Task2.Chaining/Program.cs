/*
 * 2.	Write a program, which creates a chain of four Tasks.
 * First Task – creates an array of 10 random integer.
 * Second Task – multiplies this array with another random integer.
 * Third Task – sorts this array by ascending.
 * Fourth Task – calculates the average value. All this tasks should print the values to console.
 */
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiThreading.Task2.Chaining;

static class Program
{
    static void Main()
    {
        Console.WriteLine(".Net Mentoring Program. MultiThreading V1 ");
        Console.WriteLine("2. Write a program, which creates a chain of four Tasks.");
        Console.WriteLine("First Task – creates an array of 10 random integer.");
        Console.WriteLine("Second Task – multiplies this array with another random integer.");
        Console.WriteLine("Third Task – sorts this array by ascending.");
        Console.WriteLine("Fourth Task – calculates the average value. All this tasks should print the values to console");
        Console.WriteLine();

        // Stage 1: create array and return it (no shared mutation)
        var createTask = Task.Run(() =>
        {
            var rnd = Random.Shared;
            var arr = new int[10];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = rnd.Next(1, 101);
            }

            Console.WriteLine($"Task 1 - Created array: {string.Join(", ", arr)}");
            return arr;
        });

        // Stage 2: multiply received array and return a new array
        var multiplyTask = createTask.ContinueWith(t =>
        {
            var rnd = Random.Shared;
            int multiplier = rnd.Next(1, 11);
            var multiplied = t.Result.Select(x => x * multiplier).ToArray();
            Console.WriteLine($"Task 2 - Multiplied array by {multiplier}: {string.Join(", ", multiplied)}");
            return multiplied;
        });

        // Stage 3: sort received array and return a new sorted array
        var sortTask = multiplyTask.ContinueWith(t =>
        {
            var sorted = t.Result.OrderBy(x => x).ToArray();
            Console.WriteLine($"Task 3 - Sorted array: {string.Join(", ", sorted)}");
            return sorted;
        });

        // Stage 4: compute average from received array and return it
        var avgTask = sortTask.ContinueWith(t =>
        {
            double average = t.Result.Average();
            Console.WriteLine($"Task 4 - Average value: {average}");
            return average;
        });

        // Wait for final task to complete so all outputs are printed
        avgTask.Wait();

        // Keep console open
        Console.ReadLine();
    }
}