namespace DocStream.Shared;

public class AppConfig
{
    public static string ServiceBusConnectionString => "Endpoint=sb://docprocessing-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=WUdorXDZVjCI4hVVvYHlWAl3V3vw03KrY+ASbPHWhTQ=";

    public static string QueueName => "filechunks";

    public static string InputFolder => @"C:\Users\Abdukodir_Soliev\Downloads\MessageQueue\Input";

    public static string OutputFolder => @"C:\Users\Abdukodir_Soliev\Downloads\MessageQueue\Output";

    /// <summary>
    /// Gets the default chunk size, in bytes, used for processing data in segments.
    /// </summary>
    /// <remarks>
    /// Adjust chunk size to account for Base64 encoding overhead
    /// The message size includes the base64-encoded chunk, plus JSON and headers.
    /// Base64 encoding increases size by ~33 %.For example, a 220 KB binary chunk becomes ~293 KB when base64 - encoded.
    /// The JSON envelope adds more bytes.
    /// For 256 KB: 262,144 / 1.37 ≈ 191,400 bytes (about 187 KB)
    /// </remarks>
    public static int ChunkSize => 170 * 1024; // 170kb

    public static int BarSize => 30;
}