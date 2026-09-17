using System;
using System.IO;
using System.Runtime.CompilerServices;
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
        public readonly string FilePath;
        public readonly int LineNumber;
        public readonly string MemberName;

        public LogEntry(
            GameLogLevel level,
            string message,
            Exception exception,
            string filePath,
            int lineNumber,
            string memberName)
        {
            Time = DateTime.Now;
            ThreadId = Environment.CurrentManagedThreadId;
            Level = level;
            Message = message;
            Exception = exception;
            FilePath = filePath;
            LineNumber = lineNumber;
            MemberName = memberName;
        }
    }

    private readonly object m_FileLock = new();
    private readonly bool m_WriteToFile;
    private StreamWriter m_Writer;
    private volatile bool m_Disposed;

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

    public override void OnPause()
    {
        Flush();
    }

    public override void OnDispose()
    {
        lock (m_FileLock)
        {
            if (m_Disposed)
                return;

            m_Disposed = true;
            m_Writer?.Flush();
            m_Writer?.Dispose();
            m_Writer = null;
        }
    }

    [HideInCallstack]
    public void Debug(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Write(GameLogLevel.Debug, message, null, filePath, lineNumber, memberName);
    }

    [HideInCallstack]
    public void Info(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Write(GameLogLevel.Info, message, null, filePath, lineNumber, memberName);
    }

    [HideInCallstack]
    public void Warning(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Write(GameLogLevel.Warning, message, null, filePath, lineNumber, memberName);
    }

    [HideInCallstack]
    public void Error(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Write(GameLogLevel.Error, message, null, filePath, lineNumber, memberName);
    }

    [HideInCallstack]
    public void Error(
        string message,
        Exception exception,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Write(GameLogLevel.Error, message, exception, filePath, lineNumber, memberName);
    }

    [HideInCallstack]
    public void Fatal(
        string message,
        Exception exception = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Write(GameLogLevel.Fatal, message, exception, filePath, lineNumber, memberName);
    }

    public void Flush()
    {
        lock (m_FileLock)
        {
            m_Writer?.Flush();
        }
    }

    [HideInCallstack]
    private void Write(
        GameLogLevel level,
        string message,
        Exception exception,
        string filePath,
        int lineNumber,
        string memberName)
    {
        if (m_Disposed || level < MinimumLevel)
        {
            return;
        }

        Output(new LogEntry(
            level,
            message ?? string.Empty,
            exception,
            filePath,
            lineNumber,
            memberName));
    }

    [HideInCallstack]
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

        lock (m_FileLock)
        {
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

    private static string ToUnityPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return filePath;

        filePath = filePath.Replace('\\', '/');
        var assetsIndex = filePath.IndexOf(
            "/Assets/",
            StringComparison.OrdinalIgnoreCase);

        return assetsIndex >= 0
            ? filePath.Substring(assetsIndex + 1)
            : filePath;
    }
}