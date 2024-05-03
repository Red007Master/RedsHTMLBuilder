using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WebMarkupMin.Core;

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
                        string noCompFlagPath = Path.Join(Path.GetDirectoryName(directories[j]), P.NoCompileFlag);

                        bool countainsNoCompileFlag = File.Exists(noCompFlagPath);

                        // if (directories[j].Contains("theme13") && directories[j].Contains("csharp"))
                        // {
                        //     Console.WriteLine("hhhh");
                        // }

                        if (countainsNoCompileFlag)
                        {
                            P.Logger.Log($"NoCompileFlag ([{P.NoCompileFlag}]) is found in [{directories[j]}]", LogLevel.Debug, 2);
                        }
                        else
                        {
                            P.Logger.Log($"New dir is found = [{directories[j]}]", LogLevel.Debug, 2);
                            configDirs.Add(directories[j]);
                        }
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

        WriteThemeDataOut();

        if (P.CreateProductionBuild)
        {
            using (TimeLogger tl = new TimeLogger($"Creating production build", LogLevel.Information, P.Logger, 2))
            {
                CreateProductionBuild();
            }
        }
    }

    private static void CreateProductionBuild()
    {
        string corePathIn = P.Settings.SettingsList.BuildProductionBuildFromThere;
        string corePathOut = Path.Join(corePathIn, "dev.productionOutput");

        if (Directory.Exists(corePathOut))
        {
            Directory.Delete(corePathOut);
        }

        string[] exclude = { "dev.", ".git", "README.md" };

        CopyFilesAndFolders(corePathIn, corePathOut, exclude);
    }

    public static void CopyFilesAndFolders(string copy, string copyTo, string[] excludeTriggers)
    {
        List<string> sourcefiles = GetAllFilesInPath(copy);

        sourcefiles = ExcludeLineThatCountains(sourcefiles, excludeTriggers);
        List<string> newPathFiles = new List<string>();

        for (int i = 0; i < sourcefiles.Count; i++)
        {
            newPathFiles.Add(sourcefiles[i].Replace(copy, copyTo));
        }

        HtmlMinifier htmlMinifier = new HtmlMinifier();
        var minifier = new Microsoft.Ajax.Utilities.Minifier();

        for (int i = 0; i < newPathFiles.Count; i++)
        {
            string parentDir = Path.GetDirectoryName(newPathFiles[i]);
            if (!Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);

            if (newPathFiles[i].EndsWith(".html") || newPathFiles[i].EndsWith(".js") || newPathFiles[i].EndsWith(".css"))
            {
                string fileContentIn = File.ReadAllText(sourcefiles[i]);
                string fileContentOut = "";

                if (newPathFiles[i].EndsWith(".html"))
                {
                    MarkupMinificationResult result = htmlMinifier.Minify(fileContentIn);
                    fileContentOut = result.MinifiedContent;
                }
                else if (newPathFiles[i].EndsWith(".js"))
                {
                    fileContentOut = minifier.MinifyJavaScript(fileContentIn);
                }
                else if (newPathFiles[i].EndsWith(".css"))
                {
                    fileContentOut = RemoveWhiteSpaceFromStylesheets(fileContentIn);
                }

                File.WriteAllText(newPathFiles[i], fileContentOut);
            }
            else
            {
                File.Copy(sourcefiles[i], newPathFiles[i]);
            }
        }
    }
    public static List<string> GetAllFoldersInPath(string path)
    {
        List<string> result = new List<string>();
        Queue<string> tmpQueue = new Queue<string>();

        tmpQueue.Enqueue(path);

        while (tmpQueue.Count > 0)
        {
            string current = tmpQueue.Dequeue();

            result.Add(current);

            string[] files = Directory.GetDirectories(current);

            for (int i = 0; i < files.Length; i++)
                tmpQueue.Enqueue(files[i]);
        }

        return result;
    }

    public static List<string> GetAllFilesInPath(string path)
    {
        List<string> result = new List<string>();

        List<string> foldersInPath = GetAllFoldersInPath(path);

        for (int i = foldersInPath.Count - 1; i >= 0; i--)
        {
            result.AddRange(Directory.GetFiles(foldersInPath[i]));
        }

        return result;
    }

    public static List<string> ExcludeLineThatCountains(List<string> lines, string[] excludeTriggers)
    {
        List<string> result = new List<string>();

        for (int i = 0; i < lines.Count; i++)
        {
            if (!ContainsAnyString(lines[i], excludeTriggers))
            {
                result.Add(lines[i]);
            }
        }

        return result;
    }

    public static bool ContainsAnyString(string input, string[] searchStrings)
    {
        foreach (string searchString in searchStrings)
        {
            if (input.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1)
                return true; // Found a match
        }

        return false; // No matches found
    }



    public static string RemoveWhiteSpaceFromStylesheets(string body)
    {
        body = Regex.Replace(body, @"[a-zA-Z]+#", "#");
        body = Regex.Replace(body, @"[\n\r]+\s*", string.Empty);
        body = Regex.Replace(body, @"\s+", " ");
        body = Regex.Replace(body, @"\s?([:,;{}])\s?", "$1");
        body = body.Replace(";}", "}");
        body = Regex.Replace(body, @"([\s:]0)(px|pt|%|em)", "$1");

        // Remove comments from CSS

        body = Regex.Replace(body, @"/\*[\d\D]*?\*/", string.Empty);

        return body;
    }


    public static void WriteThemeDataOut()
    {
        List<List<string>> processedThemesFinal = new List<List<string>>();

        for (int i = 0; i < P.ProcessedThemes.Count; i++)
        {
            if (!P.ProcessedThemes[i][0][0].Contains("dev"))
            {
                List<IndexedStringArray> tmpIndexStringList = new List<IndexedStringArray>();
                tmpIndexStringList = new List<IndexedStringArray>();
                tmpIndexStringList.Add(new IndexedStringArray(P.ProcessedThemes[i][0], 0));

                for (int j = 1; j < P.ProcessedThemes[i].Count; j++)
                {
                    tmpIndexStringList.Add(new IndexedStringArray(P.ProcessedThemes[i][j]));
                }

                // Create a custom comparer instance
                var comparer = new IndexedStringArrayComparer();

                // Sort the list using the custom comparer
                tmpIndexStringList.Sort(comparer);


                List<string> tmpList = new List<string>();


                int maxTitleLenght = 0;
                int maxSummaryLenght = 0;

                for (int j = 0; j < tmpIndexStringList.Count; j++)
                {
                    if (tmpIndexStringList[j].Value[0].Length > maxTitleLenght)
                    {
                        maxTitleLenght = tmpIndexStringList[j].Value[0].Length;
                    }
                    if (tmpIndexStringList[j].Value[1].Length > maxSummaryLenght)
                    {
                        maxSummaryLenght = tmpIndexStringList[j].Value[1].Length;
                    }
                }

                for (int j = 0; j < tmpIndexStringList.Count; j++)
                {
                    string numberAsString = Convert.ToString(tmpIndexStringList[j].Index);

                    if (numberAsString.Length < 2)
                    {
                        numberAsString = "0" + numberAsString;
                    }

                    int addLenghtTitle = maxTitleLenght - tmpIndexStringList[j].Value[0].Length;
                    int addLenghtSummary = maxSummaryLenght - tmpIndexStringList[j].Value[1].Length;

                    tmpList.Add($"[{numberAsString}]---[{tmpIndexStringList[j].Value[0]}{new string('-', addLenghtTitle)}|-|{tmpIndexStringList[j].Value[1]}{new string('-', addLenghtSummary)}]");
                }

                processedThemesFinal.Add(tmpList);
            }
        }

        try
        {
            string writeFilePath = P.PathDirs.ThemeList;
            File.Delete(writeFilePath);
            // Create or open the file for writing
            using (StreamWriter writer = new StreamWriter(writeFilePath, true)) // Set 'true' to append to an existing file
            {
                for (int i = 0; i < processedThemesFinal.Count; i++)
                {
                    if (i != 0) writer.WriteLine("\n");
                    writer.WriteLine(processedThemesFinal[i][0]);

                    for (int j = 1; j < processedThemesFinal[i].Count; j++)
                    {
                        writer.WriteLine(processedThemesFinal[i][j]);
                    }
                }

            }
        }
        catch (IOException e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }

    }

    public class IndexedStringArrayComparer : IComparer<IndexedStringArray>
    {
        public int Compare(IndexedStringArray x, IndexedStringArray y)
        {
            return x.Index.CompareTo(y.Index);
        }
    }


    public class IndexedStringArray
    {
        public IndexedStringArray(string[] value)
        {
            Value = value;
            Index = ExtractNumbers(value[0]);
        }

        public IndexedStringArray(string[] value, int index)
        {
            Value = value;
            Index = index;
        }

        public int Index { get; set; }
        public string[] Value { get; set; }
    }

    static int ExtractNumbers(string input)
    {
        Match match = Regex.Match(input, @"\d+");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        return int.MaxValue; // If there's no number, place it at the end
    }

    public static InfoMaterialThemeHtmlFile LoadInfoMaterialThemeHtmlFile(string configPath, string textFormaterPath, string globalTextFormaterPath, string stylesPath, string scriptsPath, string additionalHeadContentPath, string headerPath, string footerPath, string additionalMainDivContentPath, int themeNumber)
    {
        string infoMaterialThemeConfig = File.ReadAllText(configPath);

        string TextFormater = File.ReadAllText(textFormaterPath);
        string GlobalTextFormater = File.ReadAllText(globalTextFormaterPath);

        List<string> styles = File.ReadAllLines(stylesPath).ToList();
        List<string> scripts = File.ReadAllLines(scriptsPath).ToList();
        List<string> additionalHeadContent = File.ReadAllLines(additionalHeadContentPath).ToList();

        string header = File.ReadAllText(headerPath);
        string footer = File.ReadAllText(footerPath);

        string additionalMainDivContent = File.ReadAllText(additionalMainDivContentPath);

        return new InfoMaterialThemeHtmlFile(styles, scripts, additionalHeadContent, infoMaterialThemeConfig, TextFormater, GlobalTextFormater, header, footer, additionalMainDivContent, themeNumber);
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



                string parentThemeNameTmp = MoveDirectoriesDown(coreDir, 3);
                string parentThemeName = GetLastFolderName(parentThemeNameTmp);

                bool parentNameIsAlredyInList = false;
                for (int i = 0; i < P.ProcessedThemes.Count; i++)
                {
                    if (P.ProcessedThemes[i][0][0] == parentThemeName)
                    {
                        parentNameIsAlredyInList = true;
                        break;
                    }
                }

                if (!parentNameIsAlredyInList)
                {
                    List<string[]> tmpList = new List<string[]>();
                    tmpList.Add(new string[] { parentThemeName, "general-theme-name" });
                    P.ProcessedThemes.Add(tmpList);
                }


                InfoMaterialThemeProjectDirs infoMaterialThemeProjectDirs = new InfoMaterialThemeProjectDirs(coreDir);

                string[] pathArray = coreDir.Split(Path.DirectorySeparatorChar);
                string themeFolderName = pathArray[pathArray.Length - 3];
                int themeNumber = RemoveNonNumericCharactersAndReturnNumberOrZero(themeFolderName);

                infoMaterialThemeHtmlFile = LoadInfoMaterialThemeHtmlFile(infoMaterialThemeProjectDirs.InfoMaterialThemeConfig, infoMaterialThemeProjectDirs.TextFormater, P.Settings.SettingsList.GlobalHtmlTextFormaterConfig, infoMaterialThemeProjectDirs.Styles, infoMaterialThemeProjectDirs.Scripts, infoMaterialThemeProjectDirs.AdditionalHeadContent, infoMaterialThemeProjectDirs.Header, infoMaterialThemeProjectDirs.Footer, infoMaterialThemeProjectDirs.AdditionalMainDivContent, themeNumber);
                infoMaterialTaskHtmlFile = LoadInfoMaterialThemeHtmlFile(infoMaterialThemeProjectDirs.InfoMaterialTaskConfig, infoMaterialThemeProjectDirs.TextFormater, P.Settings.SettingsList.GlobalHtmlTextFormaterConfig, infoMaterialThemeProjectDirs.Styles, infoMaterialThemeProjectDirs.Scripts, infoMaterialThemeProjectDirs.AdditionalHeadContent, infoMaterialThemeProjectDirs.Header, infoMaterialThemeProjectDirs.Footer, infoMaterialThemeProjectDirs.AdditionalMainDivContent, themeNumber);
                infoMaterialResultHtmlFile = LoadInfoMaterialThemeHtmlFile(infoMaterialThemeProjectDirs.InfoMaterialResultConfig, infoMaterialThemeProjectDirs.TextFormater, P.Settings.SettingsList.GlobalHtmlTextFormaterConfig, infoMaterialThemeProjectDirs.Styles, infoMaterialThemeProjectDirs.Scripts, infoMaterialThemeProjectDirs.AdditionalHeadContent, infoMaterialThemeProjectDirs.Header, infoMaterialThemeProjectDirs.Footer, infoMaterialThemeProjectDirs.AdditionalMainDivContent, themeNumber);

                for (int i = 0; i < P.ProcessedThemes.Count; i++)
                {
                    if (P.ProcessedThemes[i][0][0] == parentThemeName)
                    {
                        char[] charsToTrim = { ' ', '\t', '\n', '\r', '\0', '\x3000' }; // Add any other characters you want to remove

                        string toAddTitle = infoMaterialThemeHtmlFile.Title.TrimEnd(charsToTrim);
                        string toAddSummary = infoMaterialThemeHtmlFile.Summary.TrimEnd(charsToTrim);

                        P.ProcessedThemes[i].Add(new string[] { infoMaterialThemeHtmlFile.Title, infoMaterialThemeHtmlFile.Summary });
                    }
                }

                loaded = true;
            }
            else
            {
                P.Logger.Log($"There is no such dir.", LogLevel.Error, 3);
            }
        }

        if (loaded)
        {
            CompileAndSaveIfConfigIsNewer(infoMaterialThemeHtmlFile, coreDir, "theme.html");
            CompileAndSaveIfConfigIsNewer(infoMaterialTaskHtmlFile, coreDir, "tasks.html");
            CompileAndSaveIfConfigIsNewer(infoMaterialResultHtmlFile, coreDir, "result.html");
            WriteWhome(infoMaterialThemeHtmlFile, coreDir, "whome.json");

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
        else
        {
            return -1;
        }
    }

    public static bool CompileAndSaveIfConfigIsNewer(InfoMaterialThemeHtmlFile infoMaterialThemeHtmlFile, string coreDir, string fileName)
    {
        bool compiledAndSaved = false;

        string htmlSavePath = Path.Combine(MoveDirectoriesDown(coreDir, 2), fileName);
        string[] array = fileName.Split('.');
        string[] themeCompileHashFileNameArray = P.Settings.SettingsList.ThemeCompileHashes.Split('.');
        string fileNameStart = themeCompileHashFileNameArray[0];
        string fileNameMid = array[0].FirstCharToUpper();
        string fileNameEnd = "." + themeCompileHashFileNameArray[1];
        string themesHashesFileName = fileNameStart + fileNameMid + fileNameEnd;
        string themeCompileHashesPath = Path.Combine(coreDir, themesHashesFileName);
        ThemesHashes oldThemesHashes = new ThemesHashes();

        if (File.Exists(themeCompileHashesPath))
        {
            oldThemesHashes = JsonConvert.DeserializeObject<ThemesHashes>(File.ReadAllText(themeCompileHashesPath));
        }

        P.Logger.Log($"CompileAndSaveIfConfigIsNewer - working with [{fileName}]", LogLevel.Debug, 2);
        if (!(infoMaterialThemeHtmlFile.ThemesHashes == oldThemesHashes) || !File.Exists(htmlSavePath))
        {
            compiledAndSaved = true;
            using (new TimeLogger("Config hash change, proceeding to compile and save file", LogLevel.Debug, P.Logger, 3))
            {
                using (new TimeLogger("Compiling - [" + fileName + "]", LogLevel.Debug, P.Logger, 4))
                {
                    infoMaterialThemeHtmlFile.Compile();
                }
                using (new TimeLogger($"Saving - [{fileName}] and [{themesHashesFileName}]", LogLevel.Debug, P.Logger, 3))
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
    public static bool WriteWhome(InfoMaterialThemeHtmlFile infoMaterialThemeHtmlFile, string coreDir, string fileName)
    {
        bool compiledAndSaved = false;

        Whome whome = new Whome(infoMaterialThemeHtmlFile);

        string htmlSavePathCore = MoveDirectoriesDown(coreDir, 2);
        string whomeSavePath = Path.Combine(htmlSavePathCore, fileName);

        string whomeSerial = JsonConvert.SerializeObject(whome, Formatting.Indented);

        File.WriteAllText(whomeSavePath, whomeSerial);

        return compiledAndSaved;
    }
    static string GetLastFolderName(string path)
    {
        // Use Path class to split the path into individual directory parts
        string[] parts = path.Split(Path.DirectorySeparatorChar);

        // Check if the path is empty or consists of only separators
        if (parts.Length == 0 || string.IsNullOrEmpty(parts[parts.Length - 1]))
        {
            return "Invalid Path";
        }

        // Return the last part of the path, which is the last folder name
        return parts[parts.Length - 1];
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
