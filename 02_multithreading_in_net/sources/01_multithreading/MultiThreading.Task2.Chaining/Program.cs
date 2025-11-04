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

        // feel free to add your code

        var random = new Random();
        var numbers = new int[10];
        var task1 = Task.Run(() =>
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.Next(1, 101);
            }
            Console.WriteLine($"Task 1 - Created array: {string.Join(", ", numbers)}");
        });

        var task2 = task1.ContinueWith(t =>
        {
            int multiplier = random.Next(1, 11);
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] *= multiplier;
            }
            Console.WriteLine($"Task 2 - Multiplied array by {multiplier}: {string.Join(", ", numbers)}");
        });

        var task3 = task2.ContinueWith(t =>
        {
            Array.Sort(numbers);
            Console.WriteLine($"Task 3 - Sorted array: {string.Join(", ", numbers)}");
        });

        var task4 = task3.ContinueWith(t =>
        {
            double average = numbers.Average();
            Console.WriteLine($"Task 4 - Average value: {average}");
        });

        Console.ReadLine();
    }
}