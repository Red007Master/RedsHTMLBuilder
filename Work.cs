using System.Text.RegularExpressions;
using Newtonsoft.Json;

internal class Work
{
    public static void MainVoid()
    {
        List<string> configDirs = new List<string>();

        List<string> lookForConfigsIn = new List<string>();

        string[] separators = { "&&" };
        lookForConfigsIn = P.Settings.SettingsList.DirsToScan.Split(separators, System.StringSplitOptions.RemoveEmptyEntries).ToList();

        using (TimeLogger tl = new TimeLogger($"Searching for dirs to process.", LogLevel.Information, P.Logger, 1))
        {
            for (int i = 0; i < lookForConfigsIn.Count; i++)
            {
                string[] directories = Directory.GetDirectories(lookForConfigsIn[i], "*", System.IO.SearchOption.AllDirectories);

                for (int j = 0; j < directories.Length; j++)
                {
                    string lastFolderName = new DirectoryInfo(directories[j]).Name;
                    if (lastFolderName == P.Settings.SettingsList.ConfigFolderName)
                    {
                        P.Logger.Log($"New dir is found = [{directories[j]}]", LogLevel.Debug, 2);
                        configDirs.Add(directories[j]);
                    }
                }
            }
        }

        using (TimeLogger tl = new TimeLogger($"Loading and Compiling themes, total count=[{configDirs.Count}]", LogLevel.Information, P.Logger, 1))
        {
            for (int i = 0; i < configDirs.Count; i++)
            {
                CheckedDirAndLoadInfoMaterialThemesIfPresent(configDirs[i]);
            }
        }
    }

    public static InfoMaterialThemeHtmlFile LoadInfoMaterialThemeHtmlFile(string configPath, string textFormaterPath, string stylesPath, string scriptsPath, string additionalHeadContentPath, string headerPath, string footerPath, string additionalMainDivContentPath, int themeNumber)
    {
        string infoMaterialThemeConfig = File.ReadAllText(configPath);
        string TextFormater = File.ReadAllText(textFormaterPath);

        List<string> styles = File.ReadAllLines(stylesPath).ToList();
        List<string> scripts = File.ReadAllLines(scriptsPath).ToList();
        List<string> additionalHeadContent = File.ReadAllLines(additionalHeadContentPath).ToList();

        string header = File.ReadAllText(headerPath);
        string footer = File.ReadAllText(footerPath);

        string additionalMainDivContent = File.ReadAllText(additionalMainDivContentPath);

        return new InfoMaterialThemeHtmlFile(styles, scripts, additionalHeadContent, infoMaterialThemeConfig, TextFormater, header, footer, additionalMainDivContent, themeNumber);
    }

    public static bool CheckedDirAndLoadInfoMaterialThemesIfPresent(string coreDir)
    {
        bool result = false;
        bool loaded = false;

        InfoMaterialThemeHtmlFile infoMaterialThemeHtmlFile = new InfoMaterialThemeHtmlFile();
        InfoMaterialThemeHtmlFile infoMaterialTaskHtmlFile = new InfoMaterialThemeHtmlFile();
        InfoMaterialThemeHtmlFile infoMaterialResultHtmlFile = new InfoMaterialThemeHtmlFile();

        using (TimeLogger tl = new TimeLogger($"Reading configs from [{coreDir}]", LogLevel.Information, P.Logger, 2))
        {
            if (Directory.Exists(coreDir))
            {
                P.Logger.Log($"Dir exist, proceeding:", LogLevel.Information, 3);

                InfoMaterialThemeProjectDirs infoMaterialThemeProjectDirs = new InfoMaterialThemeProjectDirs(coreDir);

                string[] pathArray = coreDir.Split(Path.DirectorySeparatorChar);
                string themeFolderName = pathArray[pathArray.Length - 3];
                int themeNumber = RemoveNonNumericCharactersAndReturnNumberOrZero(themeFolderName);

                infoMaterialThemeHtmlFile = LoadInfoMaterialThemeHtmlFile(infoMaterialThemeProjectDirs.InfoMaterialThemeConfig, infoMaterialThemeProjectDirs.TextFormater, infoMaterialThemeProjectDirs.Styles, infoMaterialThemeProjectDirs.Scripts, infoMaterialThemeProjectDirs.AdditionalHeadContent, infoMaterialThemeProjectDirs.Header, infoMaterialThemeProjectDirs.Footer, infoMaterialThemeProjectDirs.AdditionalMainDivContent, themeNumber);
                infoMaterialTaskHtmlFile = LoadInfoMaterialThemeHtmlFile(infoMaterialThemeProjectDirs.InfoMaterialTaskConfig, infoMaterialThemeProjectDirs.TextFormater, infoMaterialThemeProjectDirs.Styles, infoMaterialThemeProjectDirs.Scripts, infoMaterialThemeProjectDirs.AdditionalHeadContent, infoMaterialThemeProjectDirs.Header, infoMaterialThemeProjectDirs.Footer, infoMaterialThemeProjectDirs.AdditionalMainDivContent, themeNumber);
                infoMaterialResultHtmlFile = LoadInfoMaterialThemeHtmlFile(infoMaterialThemeProjectDirs.InfoMaterialResultConfig, infoMaterialThemeProjectDirs.TextFormater, infoMaterialThemeProjectDirs.Styles, infoMaterialThemeProjectDirs.Scripts, infoMaterialThemeProjectDirs.AdditionalHeadContent, infoMaterialThemeProjectDirs.Header, infoMaterialThemeProjectDirs.Footer, infoMaterialThemeProjectDirs.AdditionalMainDivContent, themeNumber);

                loaded = true;
            }
            else
            {
                P.Logger.Log($"There is no such dir.", LogLevel.Error, 3);
            }
        }

        if (loaded)
        {
            CompileAndSaveIfConfigIfNewer(infoMaterialThemeHtmlFile, coreDir, "theme.html");
            CompileAndSaveIfConfigIfNewer(infoMaterialTaskHtmlFile, coreDir, "tasks.html");
            CompileAndSaveIfConfigIfNewer(infoMaterialResultHtmlFile, coreDir, "result.html");

            result = true;
        }

        return result;
    }

    static int RemoveNonNumericCharactersAndReturnNumberOrZero(string input)
    {
        string pattern = "[^0-9]";
        string replacement = "";
        string result = Regex.Replace(input, pattern, replacement);

        if (result.Length > 0)
        {
            return Convert.ToInt32(result);
        }
        else{
            return -1;
        }
    }

    public static bool CompileAndSaveIfConfigIfNewer(InfoMaterialThemeHtmlFile infoMaterialThemeHtmlFile, string coreDir, string fileName)
    {
        bool compiledAndSaved = false;

        string htmlSavePathCore = MoveDirectoriesDown(coreDir, 2);
        string htmlSavePath = Path.Combine(htmlSavePathCore, fileName);

        string[] fileNameArray = fileName.Split('.');
        string[] themeCompileHashFileNameArray = P.Settings.SettingsList.ThemeCompileHashes.Split('.');

        string fileNameStart = themeCompileHashFileNameArray[0];
        string fileNameMid = fileNameArray[0].FirstCharToUpper();
        string fileNameEnd = "." + themeCompileHashFileNameArray[1];

        string themesHashesFileName = fileNameStart + fileNameMid + fileNameEnd;

        string themeCompileHashesPath = Path.Combine(coreDir, themesHashesFileName);

        ThemesHashes oldThemesHashes = new ThemesHashes();
        if (File.Exists(themeCompileHashesPath))
        {
            oldThemesHashes = JsonConvert.DeserializeObject<ThemesHashes>(File.ReadAllText(themeCompileHashesPath));
        }

        P.Logger.Log($"CompileAndSaveIfConfigIfNewer - working with [{fileName}]", LogLevel.Debug, 2);
        if (!(infoMaterialThemeHtmlFile.ThemesHashes == oldThemesHashes) || !(File.Exists(htmlSavePath)))
        {
            compiledAndSaved = true;

            using (TimeLogger tl = new TimeLogger("Config hash change, proceeding to compile and save file", LogLevel.Debug, P.Logger, 3))
            {
                using (TimeLogger tl2 = new TimeLogger($"Compiling - [{fileName}]", LogLevel.Debug, P.Logger, 4))
                {
                    infoMaterialThemeHtmlFile.Compile();
                }
                using (TimeLogger tl2 = new TimeLogger($"Saving - [{fileName}] and [{themesHashesFileName}]", LogLevel.Debug, P.Logger, 3))
                {
                    File.WriteAllText(htmlSavePath, infoMaterialThemeHtmlFile.ToString());

                    string serial = JsonConvert.SerializeObject(infoMaterialThemeHtmlFile.ThemesHashes, Formatting.Indented);
                    File.WriteAllText(themeCompileHashesPath, serial);
                }
            }
        }
        else
        {
            P.Logger.Log("Config hash is the same, proceeding without changes", LogLevel.Debug, 3);
        }

        return compiledAndSaved;
    }

    public static string MoveDirectoriesDown(string path, int n)
    {
        // Iterate N times to move down the directories
        for (int i = 0; i < n; i++)
        {
            // Get the parent directory of the current path
            DirectoryInfo parentDir = Directory.GetParent(path);

            // Check if a parent directory exists
            if (parentDir != null)
            {
                // Set the parent directory as the new path
                path = parentDir.FullName;
            }
            else
            {
                // If no parent directory exists, break out of the loop
                break;
            }
        }

        // At this point, 'path' contains the new directory path after moving down N directories
        // You can perform further operations using the new path
        return path;
    }
}

public class InfoMaterialThemeProjectDirs
{
    public string InfoMaterialThemeConfig { get; set; }
    public string InfoMaterialTaskConfig { get; set; }
    public string InfoMaterialResultConfig { get; set; }

    public string TextFormater { get; set; }

    public string Styles { get; set; }
    public string Scripts { get; set; }

    public string AdditionalHeadContent { get; set; }
    public string AdditionalMainDivContent { get; set; }


    public string Header { get; set; }
    public string Footer { get; set; }

    public InfoMaterialThemeProjectDirs(string corePath)
    {
        bool anyFileIsMising = true;

        InfoMaterialThemeConfig = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.CurrentThemeConfig));
        InfoMaterialTaskConfig = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.CurrentTaskConfig));
        InfoMaterialResultConfig = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.CurrentResultConfig));

        TextFormater = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.CurrentHtmlTextFormaterConfig));

        Styles = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.StylesToUse));
        Scripts = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.ScriptsToUse));

        AdditionalHeadContent = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.AdditionalHeadContent));
        AdditionalMainDivContent = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.AdditionalMainDivContent));

        Header = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.HeaderElement));
        Footer = IfPathExistReturnPathElseNull(Path.Combine(corePath, P.Settings.SettingsList.FooterElement));

        anyFileIsMising = InfoMaterialThemeConfig == null || InfoMaterialTaskConfig == null || InfoMaterialResultConfig == null || TextFormater == null || Styles == null || Scripts == null || AdditionalHeadContent == null || AdditionalMainDivContent == null || Header == null || Footer == null;

        if (anyFileIsMising)
        {
            P.Logger.Log($"Files is missing in [{corePath}]!", LogLevel.FatalError, 3);
            string filesData =
                $"                      [InfoMaterialThemeConfig-------]=[{InfoMaterialThemeConfig}]\n" +
                $"                      [InfoMaterialTaskConfig--------]=[{InfoMaterialTaskConfig}]\n" +
                $"                      [InfoMaterialResultConfig------]=[{InfoMaterialResultConfig}]\n" +
                $"                      [TextFormater--------------]=[{TextFormater}]\n" +
                $"                      [Styles--------------------]=[{Styles}]\n" +
                $"                      [Scripts-------------------]=[{Scripts}]\n" +
                $"                      [AdditionalHeadContent-----]=[{AdditionalHeadContent}]\n" +
                $"                      [AdditionalMainDivContent--]=[{AdditionalMainDivContent}]\n" +
                $"                      [Header--------------------]=[{Header}]\n" +
                $"                      [Footer--------------------]=[{Footer}]";

            P.Logger.Log($"Files:\n{filesData}", LogLevel.FatalError, 4);
        }
        else
        {
            P.Logger.Log($"All files is present in [{corePath}]!", LogLevel.Information, 3);
        }

        if (InfoMaterialThemeConfig == null || InfoMaterialTaskConfig == null || InfoMaterialResultConfig == null)
        {
            if (InfoMaterialThemeConfig == null)
            {
                P.Logger.Log("InfoMaterialThemeConfig is missing, writing dummy.", LogLevel.Information, 5);
                string json = JsonConvert.SerializeObject(new InfoMaterialThemeConfig(InfoMaterialThemeConfigType.Theme));

                File.WriteAllText(Path.Combine(corePath, P.Settings.SettingsList.CurrentThemeConfig), json);
            }
            if (InfoMaterialTaskConfig == null)
            {
                P.Logger.Log("InfoMaterialTaskConfig is missing, writing dummy.", LogLevel.Information, 5);
                string json = JsonConvert.SerializeObject(new InfoMaterialThemeConfig(InfoMaterialThemeConfigType.Tasks));

                File.WriteAllText(Path.Combine(corePath, P.Settings.SettingsList.CurrentTaskConfig), json);
            }
            if (InfoMaterialResultConfig == null)
            {
                P.Logger.Log("InfoMaterialResultConfig is missing, writing dummy.", LogLevel.Information, 5);
                string json = JsonConvert.SerializeObject(new InfoMaterialThemeConfig(InfoMaterialThemeConfigType.Results));

                File.WriteAllText(Path.Combine(corePath, P.Settings.SettingsList.CurrentResultConfig), json);
            }
        }
    }

    public string IfPathExistReturnPathElseNull(string path)
    {
        bool exist = File.Exists(path);

        if (exist)
        {
            return path;
        }
        else
        {
            return null;
        }
    }
}

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
}