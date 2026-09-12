using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using UnityEngine;

public enum GameLogLevel : byte
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public sealed class LogSystem : LogicSystem
{
    private readonly struct LogEntry
    {
        public readonly DateTime Time;
        public readonly int ThreadId;
        public readonly GameLogLevel Level;
        public readonly string Message;
        public readonly Exception Exception;

        public LogEntry(GameLogLevel level, string message, Exception exception)
        {
            Time = DateTime.Now;
            ThreadId = Environment.CurrentManagedThreadId;
            Level = level;
            Message = message;
            Exception = exception;
        }
    }

    private readonly ConcurrentQueue<LogEntry> m_Entries = new();

    private readonly bool m_WriteToFile;
    private StreamWriter m_Writer;
    private bool m_Disposed;

    public GameLogLevel MinimumLevel { get; set; }
    public string LogFilePath { get; private set; }

    public LogSystem(GameLogLevel minimumLevel = GameLogLevel.Debug, bool writeToFile = true)
    {
        MinimumLevel = minimumLevel;
        m_WriteToFile = writeToFile;
    }

    public override void OnInit()
    {
        if (!m_WriteToFile)
        {
            return;
        }

        try
        {
            var directory = Path.Combine(Application.persistentDataPath, "Logs");

            Directory.CreateDirectory(directory);

            var fileName = $"Game_{DateTime.Now:yyyyMMdd_HHmmss}.log";

            LogFilePath = Path.Combine(directory, fileName);

            var stream = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

            m_Writer = new StreamWriter(stream, new UTF8Encoding(false));
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError(
                $"创建日志文件失败：{exception}");
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        Flush();
    }

    public override void OnPause()
    {
        Flush();
    }

    public override void OnDispose()
    {
        if (m_Disposed)
            return;

        m_Disposed = true;

        Flush();

        m_Writer?.Flush();
        m_Writer?.Dispose();
        m_Writer = null;
    }

    public void Debug(string message)
    {
        Write(GameLogLevel.Debug, message);
    }

    public void Info(string message)
    {
        Write(GameLogLevel.Info, message);
    }

    public void Warning(string message)
    {
        Write(GameLogLevel.Warning, message);
    }

    public void Error(string message)
    {
        Write(GameLogLevel.Error, message);
    }

    public void Error(
        string message,
        Exception exception)
    {
        Write(GameLogLevel.Error, message, exception);
    }

    public void Fatal(
        string message,
        Exception exception = null)
    {
        Write(GameLogLevel.Fatal, message, exception);
    }

    public void Flush()
    {
        while (m_Entries.TryDequeue(out var entry))
        {
            Output(entry);
        }

        m_Writer?.Flush();
    }

    private void Write(GameLogLevel level, string message, Exception exception = null)
    {
        if (m_Disposed || level < MinimumLevel)
        {
            return;
        }

        m_Entries.Enqueue(new LogEntry(level, message ?? string.Empty, exception));
    }

    private void Output(in LogEntry entry)
    {
        var formattedMessage =
            $"[{entry.Time:yyyy-MM-dd HH:mm:ss.fff}]" +
            $"[{entry.Level.ToString().ToUpperInvariant()}]" +
            $"[Thread:{entry.ThreadId}] " +
            entry.Message;

        if (entry.Exception != null)
        {
            formattedMessage +=
                Environment.NewLine +
                entry.Exception;
        }

        switch (entry.Level)
        {
            case GameLogLevel.Debug:
            case GameLogLevel.Info:
                UnityEngine.Debug.Log(formattedMessage);
                break;

            case GameLogLevel.Warning:
                UnityEngine.Debug.LogWarning(formattedMessage);
                break;

            case GameLogLevel.Error:
            case GameLogLevel.Fatal:
                UnityEngine.Debug.LogError(formattedMessage);
                break;
        }

        if (m_Writer == null)
            return;

        try
        {
            m_Writer.WriteLine(formattedMessage);
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError(
                $"写入日志文件失败：{exception}");

            m_Writer.Dispose();
            m_Writer = null;
        }
    }
}