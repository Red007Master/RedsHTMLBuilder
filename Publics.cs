﻿internal class P
{
    public static Logger Logger { get; set; }
    public static Settings Settings { get; set; }

    public static List<List<string[]>> ProcessedThemes { get; set; } = new List<List<string[]>>();

    public static string ExecutableCreationTime { get; set; } = String.Empty;
    public static string ExecutableLastWriteTime { get; set; } = String.Empty;
    public static string ExecutableHashSHA256 { get; set; } = String.Empty;

    public static string AppStartDate { get; set; } = String.Empty;

    public static bool ComplieCSharpCode {get; set;} = true;
    public static bool CreateProductionBuild {get;set;} = false;
    public static bool ConfigureGator {get;set;} = false;
    public static string[] GatorAddTargets { get; set; } = new string[0];

    public static string SettingsHashSHA256 { get; set; } = String.Empty;

    public static PathNames PathNames { get; set; } = new PathNames();
    public static PathDirs PathDirs { get; set; } = new PathDirs();

    public static string NoCompileFlag {get; set;} = "no-comp.flag";
}


public class PathCoreClass
{
    public string Core { get; set; }

    public string MainSettings { get; set; }
    public string Log { get; set; }

    public string Util { get; set; }
    public string SharpBuild { get; set; }

    public string ThemeList { get; set; }
}

public class PathNames : PathCoreClass
{
    public PathNames()
    {
        Core = "RedsHTMLBuilder";

        MainSettings = "Settings.txt";                                              //f12
        Log = "Log.log";                                                            //f12

        Util = "Util";                                                              //d12
        SharpBuild = "SharpBuild";                                                  //d123

        ThemeList = "ThemeList.txt";                                                //f12
    }
}
public class PathDirs : PathCoreClass
{

    public void SetFromExecutionPath(string inputExecutionPath, PathNames inputPathNames)
    {
        this.Core = GetCorePath(inputExecutionPath, inputPathNames.Core);

        this.Log = this.Core + Path.DirectorySeparatorChar + inputPathNames.Log;

        this.Util = this.Core + Path.DirectorySeparatorChar + inputPathNames.Util;
        this.SharpBuild = this.Util + Path.DirectorySeparatorChar + inputPathNames.SharpBuild;

        this.MainSettings = this.Core + Path.DirectorySeparatorChar + inputPathNames.MainSettings;

        this.ThemeList = this.Core + Path.DirectorySeparatorChar + inputPathNames.ThemeList;
    }

    private static string GetCorePath(string inputCurrentPath, string inputCorePathName)
    {
        string corePathBuffer = "";
        string[] currentPathAsArray;
        bool corePathIsDetected = false;

        currentPathAsArray = inputCurrentPath.Split(Convert.ToChar(Path.DirectorySeparatorChar));

        for (int i = 0; i < currentPathAsArray.Length; i++)
        {
            corePathBuffer += currentPathAsArray[i] + Path.DirectorySeparatorChar;

            if (currentPathAsArray[i] == inputCorePathName)
            {
                corePathIsDetected = true;
                break;
            }
        }

        corePathBuffer = corePathBuffer.Remove(corePathBuffer.Length - 1);

        if (!corePathIsDetected)
        {
            P.Logger.Log($"Core Path not found, CorePathName:[{inputCorePathName}]", LogLevel.FatalError);
            Console.ReadLine();
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
        else
        {
            return corePathBuffer;
        }
    }
}