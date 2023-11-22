using System.Diagnostics;

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


