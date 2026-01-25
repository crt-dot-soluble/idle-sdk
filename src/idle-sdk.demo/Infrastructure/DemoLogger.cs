using System.Diagnostics;
using System.Text.Json;

namespace IdleSdk.Demo.Infrastructure;

public static class DemoLogger
{
    private static readonly object Gate = new();
    private static string? _logFilePath;
    private static string? _workspaceLogFilePath;

    public static void Initialize(string appName)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "logs");
        Directory.CreateDirectory(root);
        _logFilePath = Path.Combine(root, "demo.log");
        var workspaceRoot = Environment.CurrentDirectory;
        if (!string.IsNullOrWhiteSpace(workspaceRoot))
        {
            var workspaceLogs = Path.Combine(workspaceRoot, "logs");
            Directory.CreateDirectory(workspaceLogs);
            _workspaceLogFilePath = Path.Combine(workspaceLogs, "demo.log");
        }
        Info("logger", "initialized", new { path = _logFilePath });
    }

    public static void Info(string component, string eventName, object? data = null)
        => Write("INFO", component, eventName, data);

    public static void Warn(string component, string eventName, object? data = null)
        => Write("WARN", component, eventName, data);

    public static void Error(string component, string eventName, Exception exception, object? data = null)
    {
        var payload = new
        {
            exception = exception.GetType().FullName,
            message = exception.Message,
            stackTrace = exception.StackTrace,
            data
        };

        Write("ERROR", component, eventName, payload);
    }

    private static void Write(string level, string component, string eventName, object? data)
    {
        var entry = new
        {
            timestamp = DateTimeOffset.UtcNow.ToString("O"),
            level,
            component,
            eventName,
            data
        };

        var line = JsonSerializer.Serialize(entry);
        Trace.WriteLine(line);
        Console.WriteLine(line);

        if (_logFilePath is null)
        {
            return;
        }

        lock (Gate)
        {
            File.AppendAllText(_logFilePath, line + Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(_workspaceLogFilePath))
            {
                File.AppendAllText(_workspaceLogFilePath, line + Environment.NewLine);
            }
        }
    }
}
