using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Log
{
    [HideInCallstack]
    public static void Debug(
        string msg,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Debug(msg, filePath, lineNumber, memberName);
    }
    
    [HideInCallstack]
    public static void Info(
        string msg,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Info(msg, filePath, lineNumber, memberName);
    }
    
    [HideInCallstack]
    public static void Warning(
        string msg,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Warning(msg, filePath, lineNumber, memberName);
    }
    
    [HideInCallstack]
    public static void Error(
        string msg,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Error(msg, filePath, lineNumber, memberName);
    }
    
    [HideInCallstack]
    public static void Error(
        string msg,
        Exception exception,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Error(msg, exception, filePath, lineNumber, memberName);
    }
    
    [HideInCallstack]
    public static void Fatal(
        string msg,
        Exception exception = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Fatal(msg, exception, filePath, lineNumber, memberName);
    }
}