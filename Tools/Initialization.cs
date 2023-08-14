using System.Reflection;

internal class Initalization
{
    public static void Start()
    {
        ConsoleInit();

        PathDirsInit();

        P.Logger = new Logger(P.PathDirs.Log, new LogSettings(LogLevel.Debug));
        P.Settings = new Settings(P.PathDirs.MainSettings);

        string execPath = Assembly.GetEntryAssembly().Location;

        using (TimeLogger tl = new TimeLogger("Getting exec info and hash", LogLevel.Information, P.Logger, 1))
        {
            P.ExecutableCreationTime = File.GetCreationTime(execPath).ToString();
            P.ExecutableLastWriteTime = File.GetLastWriteTime(execPath).ToString();
            P.ExecutableHashSHA256 = Hash.GetFileSHA256(execPath);
            P.SettingsHashSHA256 = Hash.GetFileSHA256(P.PathDirs.MainSettings);

            P.AppStartDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }

    private static void ConsoleInit()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Title = "RedsHTMLBuilder";

        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            //Console.WindowWidth = 200;
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception)
        {
            P.Logger.Log("Console.WindowWidth = 200 can't be set", LogLevel.Warning);
        }
    }
    private static void PathDirsInit()
    {
        string currentPath = Environment.CurrentDirectory;
        currentPath = @"/media/M2King1tb/Development/RedsSoft/RedsHTMLBuilder"; //DEV

        P.PathDirs.SetFromExecutionPath(currentPath, P.PathNames);
        Dir.CreateAllDirsInObject(P.PathDirs);
    }
}
