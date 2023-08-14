using System.Diagnostics;

internal class Logger
{
    public string FilePath { get; set; }
    public LogSettings LogSettings { get; set; }
    public StreamWriter SW { get; set; }

    public void Log(string iLogMassage, LogLevel iLogLevel, int iDepthLevel = 0)
    {
        string result;

        if (iDepthLevel == 0)
        {
            result = $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [{iLogLevel}]{new string(' ', SPC.Get(iLogLevel))} | [{iLogMassage}]";
        }
        else
        {
            result = $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [{iLogLevel}]{new string(' ', SPC.Get(iLogLevel))} |{new string('-', iDepthLevel)}>[{iLogMassage}]";
        }

        if (LogSettings.IsItToWrite(iLogLevel))
        {
            SW.WriteLine(result);
            SW.Flush();
        }

        Console.WriteLine(result);
    }
    public void Log(string inputLogMassage, int gapSize, LogLevel iLogLevel)
    {
        for (int i = 0; i < gapSize; i++)
        {
            Console.WriteLine();
            SW.WriteLine();
        }

        Log(inputLogMassage, iLogLevel);
    }

    public Logger(string filePath, LogSettings logSettings)
    {
        FilePath = filePath;
        LogSettings = logSettings;
        SW = new StreamWriter(FilePath, true, System.Text.Encoding.Default);
    }
}
internal class TimeLogger : IDisposable
{
    private Stopwatch Stopwatch { get; set; } = new Stopwatch();

    private string Sender { get; set; }
    private Logger Logger { get; set; }
    private LogLevel LogLevel { get; set; }
    private int DepthLevel { get; set; }

    public TimeLogger(string sender, LogLevel logLevel, Logger logger, int depthLevel = 0)
    {
        Stopwatch.Start();
        Sender = sender;
        Logger = logger;
        LogLevel = logLevel;
        DepthLevel = depthLevel;

        Logger.Log($"[{Sender}]-Try", LogLevel, DepthLevel);
    }

    public void Dispose()
    {
        Stopwatch.Start();
        Logger.Log($"[{Sender}] completed in [{Stopwatch.ElapsedMilliseconds}]ms", LogLevel, DepthLevel + 1);
    }
}

public class LogSettings
{
    public bool WriteDebug { get; set; } = true;
    public bool WriteInformation { get; set; } = true;
    public bool WriteWarning { get; set; } = true;
    public bool WriteError { get; set; } = true;
    public bool WriteFatalError { get; set; } = true;

    public void SetLogLevel(LogLevel iMinLogLevel)
    {
        if (0 <= (int)iMinLogLevel)
            WriteDebug = false;
        if (1 <= (int)iMinLogLevel)
            WriteInformation = false;
        if (2 <= (int)iMinLogLevel)
            WriteWarning = false;
        if (3 <= (int)iMinLogLevel)
            WriteError = false;
        if (4 <= (int)iMinLogLevel)
            WriteFatalError = false;
    }
    public bool IsItToWrite(LogLevel iLogLevel)
    {
        switch (iLogLevel)
        {
            case LogLevel.Debug:
                return WriteDebug;
            case LogLevel.Information:
                return WriteInformation;
            case LogLevel.Warning:
                return WriteWarning;
            case LogLevel.Error:
                return WriteError;
            case LogLevel.FatalError:
                return WriteFatalError;
            default:
                return true;
        }
    }
    public LogSettings(LogLevel iMinLogLevel)
    {
        if (0 <= (int)iMinLogLevel)
            WriteDebug = false;
        if (1 <= (int)iMinLogLevel)
            WriteInformation = false;
        if (2 <= (int)iMinLogLevel)
            WriteWarning = false;
        if (3 <= (int)iMinLogLevel)
            WriteError = false;
        if (4 <= (int)iMinLogLevel)
            WriteFatalError = false;
    }
}
public static class SPC
{
    public static int Get(LogLevel iLogLevel)
    {
        switch (iLogLevel)
        {
            case LogLevel.Debug:
                return 6;
            case LogLevel.Information:
                return 0;
            case LogLevel.Warning:
                return 4;
            case LogLevel.Error:
                return 6;
            case LogLevel.FatalError:
                return 1;
            default: return 0;
        }
    }
}

public enum LogLevel
{
    Debug = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    FatalError = 4,
}