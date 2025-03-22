
using System.Collections.Concurrent;

namespace ServerHead.Scripts;
public class Logger
{
    private static readonly Lazy<Logger> _instance = new(new Logger());
    private readonly ConcurrentQueue<string> _logQueue;
    private readonly Task _logTask;
    private const string Log_File_Path = @".\Log.log";
    private bool _isRunning;

    private Logger()
    {
        _logQueue = new ConcurrentQueue<string>();
        _isRunning = true;
        _logTask = Task.Run(ProcessLogQueue);
    }

    public static Logger Instance => _instance.Value;

    public void Log(LogLevel logLevel, string message)
    {
        string logEntry = $"{DateTime.Now:dd/MM/yy HH:mm:ss} : {logLevel.ToString()} : {message}";
        _logQueue.Enqueue(logEntry);
    }

    private async Task ProcessLogQueue()
    {
        while (_isRunning)
        {
            await Task.Delay(100); //for last second log messages have delay before Dequeue
            while (_logQueue.TryDequeue(out string? logEntry))
            {
                await File.AppendAllTextAsync(Log_File_Path, logEntry + Environment.NewLine);
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _logTask.Wait();
    }
}