namespace DocStream.CaptureService.Producer;

/// <summary> Producer </summary>
internal class Program
{
    /// <summary> Producer </summary>
    private static void Main()
    {
        Producer producer = new();
        producer.Start();
        Console.ReadKey();
    }
}