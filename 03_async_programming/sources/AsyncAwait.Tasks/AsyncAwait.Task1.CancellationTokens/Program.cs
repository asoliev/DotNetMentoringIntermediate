/*
* Study the code of this application to calculate the sum of integers from 0 to N, and then
* change the application code so that the following requirements are met:
* 1. The calculation must be performed asynchronously.
* 2. N is set by the user from the console. The user has the right to make a new boundary in the calculation process,
* which should lead to the restart of the calculation.
* 3. When restarting the calculation, the application should continue working without any failures.
*/

namespace AsyncAwait.Task1.CancellationTokens;

internal static class Program
{
    private static CancellationTokenSource? _cts;

    /// <summary> The Main method should not be changed at all.</summary>
    private static void Main()
    {
        Console.WriteLine("Mentoring program L2. Async/await.V1. Task 1");
        Console.WriteLine("Calculating the sum of integers from 0 to N.");
        Console.WriteLine("Use 'q' key to exit...");
        Console.WriteLine();

        Console.WriteLine("Enter N: ");

        var input = Console.ReadLine();
        // Fix for CS8602: Dereference of a possibly null reference.
        // Add null check for input before calling Trim().
        while (input != null && !input.Trim().Equals("Q", StringComparison.CurrentCultureIgnoreCase))
        {
            if (int.TryParse(input, out var n))
            {
                if (n < 0)
                {
                    Console.WriteLine("N must be a non-negative integer. Please try again.");
                    Console.WriteLine("Enter N: ");
                    input = Console.ReadLine();
                    continue;
                }

                StartCalculateSum(n);
            }
            else
            {
                Console.WriteLine($"Invalid integer: '{input}'. Please try again.");
                Console.WriteLine("Enter N: ");
            }

            input = Console.ReadLine();
        }

        Console.WriteLine("Press any key to continue");
        Console.ReadLine();
    }

    private static void StartCalculateSum(int n)
    {
        // Dispose previous CancellationTokenSource if exists
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // Start new calculation asynchronously
        Task.Run(() =>
        {
            try
            {
                Console.WriteLine($"The task for {n} started... Enter N to cancel the request:");
                long sum = Calculator.Calculate(n, token);
                if (!token.IsCancellationRequested)
                {
                    Console.WriteLine($"Sum for {n} = {sum}.");
                }
                else
                {
                    Console.WriteLine($"Sum for {n} cancelled...");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Sum for {n} cancelled...");
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Enter N: ");
            }
        }, token);
    }
}
