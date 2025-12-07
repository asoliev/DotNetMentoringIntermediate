namespace DocStream.ProcessingService.Consumer;

/// <summary> Consumer </summary>
public class Program
{
    /// <summary> Consumer </summary>
    public static async Task Main()
    {
        Consumer consumer = new();
        await consumer.Start();
    }
}