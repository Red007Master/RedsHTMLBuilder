using System.Reflection;
using Newtonsoft.Json;
using RedsHTMLBuilder.Tools;

internal class Initalization
{
    public static void Start(string[] startArgs)
    {
        ConsoleInit();

        PathDirsInit();

        P.Logger = new Logger(P.PathDirs.Log, new LogSettings(LogLevel.Debug));
        P.Settings = new Settings(P.PathDirs.MainSettings);

        //dev
        P.ComplieCSharpCode = false;
        if (startArgs.Contains("--nocomp"))
        {
            P.ComplieCSharpCode = false;
        }

        if (startArgs.Contains("--prodout"))
        {
            P.CreateProductionBuild = true;
        }

        if (startArgs.Contains("--gatoradd"))
        {
            P.ConfigureGator = true;

            int index = Array.IndexOf(startArgs, "--gatoradd");

            string gatorAddArgs = startArgs[index + 1];

            P.Logger.Log($"'gatoradd' arg detected, with value:[{gatorAddArgs}]", LogLevel.Information, 1);

            P.GatorAddTargets = gatorAddArgs.Split(',');
        }

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
    }
    private static void PathDirsInit()
    {
        string currentPath = Environment.CurrentDirectory;

        // currentPath = PersonalInit.GetPersonalDevPath(); //DEV

        P.PathDirs.SetFromExecutionPath(currentPath, P.PathNames);
        Dir.CreateAllDirsInObject(P.PathDirs);
    }
}
