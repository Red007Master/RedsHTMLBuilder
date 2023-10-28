using System.Diagnostics;

internal class P
{
    public static Logger Logger { get; set; }
    public static Settings Settings { get; set; }

    public static List<List<string[]>> ProcessedThemes { get; set; } = new List<List<string[]>>();

    public static string ExecutableCreationTime { get; set; } = String.Empty;
    public static string ExecutableLastWriteTime { get; set; } = String.Empty;
    public static string ExecutableHashSHA256 { get; set; } = String.Empty;

    public static string AppStartDate { get; set; } = String.Empty;

    public static bool ComplieCSharpCode {get; set;} = true;

    public static string SettingsHashSHA256 { get; set; } = String.Empty;

    public static PathNames PathNames { get; set; } = new PathNames();
    public static PathDirs PathDirs { get; set; } = new PathDirs();
}

public class CodeCompiler
{
    public static CompilationResult CompileCode(string code, string projectPath)
    {
        // Create a temporary directory to store the project
        // string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string tempDir = projectPath;
        
        Directory.CreateDirectory(tempDir);

        try
        {
            // Save the code to a temporary source file (e.g., Program.cs)
            string sourceFilePath = Path.Combine(tempDir, "Program.cs");
            File.WriteAllText(sourceFilePath, code);

            // Create a minimal .csproj file in the temporary directory
            string csprojFilePath = Path.Combine(tempDir, "TemporaryProject.csproj");
            File.WriteAllText(csprojFilePath, $@"<Project Sdk=""Microsoft.NET.Sdk""><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net7.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><PackageReference Include=""Newtonsoft.Json"" Version=""13.0.3"" /></ItemGroup></Project>
");

            // Set the working directory to the project path
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build {csprojFilePath}",
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // Check the build output for errors or warnings
                if (process.ExitCode == 0)
                {
                    return new CompilationResult(true, "Compilation successful!");
                }
                else
                {
                    return new CompilationResult(false, "Compilation failed with errors or warnings:\n" + output + errorOutput);
                }
            }
        }
        finally
        {
            // Clean up the temporary directory
            Directory.Delete(tempDir, true);
        }
    }
}

public class CompilationResult
{
    public bool Success { get; }
    public string Message { get; }

    public CompilationResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
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
        Log = "Log.txt";                                                            //f12

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