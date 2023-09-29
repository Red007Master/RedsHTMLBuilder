using RedsSettings;
using System.ComponentModel;
using System.Reflection;

public class Settings
{
    public string FilePath { get; set; }
    public int LoopCounter { get; set; } = 0;

    public SettingsList SettingsList { get; set; }
    private IDictionary<string, ISetting> SettingsDictionary = new Dictionary<string, ISetting>();

    public void ReadSettings()
    {
        if (File.Exists(FilePath))
        {
            P.Logger.Log($"SettingsFile detected, path=[{FilePath}]", LogLevel.Debug, 2);

            string[] fileContentArray = File.ReadAllLines(FilePath);
            string[] cuted1, cuted2;

            for (int i = 0; i < fileContentArray.Length; i++)
            {
                if (fileContentArray[i].Contains("|"))
                {
                    fileContentArray[i] = fileContentArray[i].Replace("[", "");
                    fileContentArray[i] = fileContentArray[i].Replace("]", "");
                    cuted1 = fileContentArray[i].Split('|');
                    cuted2 = fileContentArray[i + 1].Split('=');

                    P.Logger.Log($"Try apply [{cuted2[1]}] to [{cuted1[0]}]", LogLevel.Debug, 3);
                    try
                    {
                        SettingsDictionary[cuted1[0]].SetFromString(cuted2[1]);
                        P.Logger.Log($"[{cuted1[0]}] applyed with value = [{cuted2[1]}]", LogLevel.Debug, 4);
                    }
                    catch (System.Exception ex)
                    { P.Logger.Log($"[{cuted1[0]}] don't applyed with value = [{cuted2[1]}] ex=[{ex}]", LogLevel.Error); }
                }
            }
        }
        else
        {
            if (LoopCounter > 5)
                P.Logger.Log("RecursiveLoop", LogLevel.FatalError);

            LoopCounter++;

            P.Logger.Log($"ReadSettings: Error SettingsFile don't detected, path=[{FilePath}], applying DefaultSettings, WriteSettings: Try", LogLevel.Warning, 1);

            using (TimeLogger tl = new TimeLogger("ApplySettingsList", LogLevel.Information, P.Logger))
            {
                FromDictionaryToClass();
            }

            using (TimeLogger tl = new TimeLogger("WriteSettings", LogLevel.Information, P.Logger))
            {
                WriteSettings();
            }

            P.Logger.Log($"ReadSettings: Error SettingsFile don't detected, path=[{FilePath}], WriteSettings: Success, Initiating Recursive Call of ReadSettings", LogLevel.Debug);

            ReadSettings();
        }
    }
    public void WriteSettings()
    {
        FromClassToDictionary();

        try
        {
            File.Delete(FilePath);
        }
        catch (Exception)
        { } //TODO

        using (StreamWriter sw = new StreamWriter(FilePath, true, System.Text.Encoding.Default))
        {
            for (int i = 0; i < SettingsDictionary.Count; i++)
            {
                var kayValueBuffer = SettingsDictionary.ElementAt(i);
                SettingStr settingStrBuffer = kayValueBuffer.Value.GetSettingStr();

                sw.WriteLine($"[{settingStrBuffer.Name}|{settingStrBuffer.Description}]");
                sw.WriteLine($"{settingStrBuffer.Name}={settingStrBuffer.Value}\n");
            }
        }
    }

    private void GetDefaultSettingsList()
    {
        AddSetting<string>("AdditionalHeadContent.html", "AdditionalHeadContent", "Name of AdditionalHeadContent file.");
        AddSetting<string>("AdditionalMainDivContent.html", "AdditionalMainDivContent", "Name of AdditionalMainDivContent file.");

        AddSetting<string>("HeaderElement.html", "HeaderElement", "Name of HeaderElement file.");
        AddSetting<string>("FooterElement.html", "FooterElement", "Name of FooterElement file.");

        AddSetting<string>("currentHtmlTextFormaterConfig.json", "CurrentHtmlTextFormaterConfig", "Name of currentHtmlTextFormaterConfig file.");

        AddSetting<string>("currentThemeConfig.json", "CurrentThemeConfig", "Name of currentThemeConfig file.");
        AddSetting<string>("currentTaskConfig.json", "CurrentTaskConfig", "Name of currentTaskConfig file.");
        AddSetting<string>("currentResultConfig.json", "CurrentResultConfig", "Name of currentResultConfig file.");

        AddSetting<string>("/media/M2King1tb/Development/Projects/Webskill/Themes/WebskillThemes/courses/!dev.GlobalData/currentHtmlTextFormaterConfig.json", "GlobalHtmlTextFormaterConfig", "GlobalHtmlTextFormaterConfig path.");

        AddSetting<string>("ScriptsToUse.txt", "ScriptsToUse", "Name of ScriptsToUse file.");
        AddSetting<string>("StylesToUse.txt", "StylesToUse", "Name of StylesToUse file.");

        AddSetting<bool>(true, "AddDebugDivToHtml", "Is DebugDiv will be added to output html.");

        AddSetting<string>("themeCompileHashes.json", "ThemeCompileHashes", "Name of file that contains hashes of compile settings.");

        AddSetting<string>("ThemeConfig", "ConfigFolderName", "Name of config folders.");
        AddSetting<string>("dirsThere", "DirsToScan", "Dirs that sould be scaned for config folders separated by '&&'.");
    } //Dev set

    private void AddSetting<Tinput>(Tinput inputValue, string inputName, string inputDescription)
    {
        Setting<Tinput> buffer = new Setting<Tinput>(inputValue, inputName, inputDescription);
        SettingsDictionary.Add(buffer.Name, buffer);
    }

    private void FromClassToDictionary()
    {
        foreach (PropertyInfo propertyInfo in SettingsList.GetType().GetProperties())
        {
            var value = propertyInfo.GetValue(SettingsList);
            string name = propertyInfo.Name;

            SettingsDictionary[name].SetFromObject(value);
        }
    }
    private void FromDictionaryToClass()
    {
        foreach (KeyValuePair<string, ISetting> item in SettingsDictionary)
        {
            ISetting current = item.Value;

            var property = typeof(SettingsList).GetProperty(current.GetName());
            property.SetValue(SettingsList, current.GetValue(), null);
        }
    }

    public void ConsoleOutputSettings()
    {
        foreach (var setting in SettingsDictionary)
        {
            ISetting settingBuffer = setting.Value;
            SettingStr settingStr = settingBuffer.GetSettingStr();

            Console.WriteLine($"Name=[{settingStr.Name}][{settingStr.Value}]");
        }
        Console.WriteLine("\n");
    }

    public Settings(string filePath)
    {
        FilePath = filePath;

        SettingsList = new SettingsList();

        using (TimeLogger tl = new TimeLogger("GetDefaultSettingsList", LogLevel.Information, P.Logger, 1))
        {
            GetDefaultSettingsList();
        }

        using (TimeLogger tl = new TimeLogger("ReadSettings", LogLevel.Information, P.Logger, 1))
        {
            ReadSettings();
        }

        using (TimeLogger tl = new TimeLogger("FromDictionaryToClass", LogLevel.Information, P.Logger, 1))
        {
            FromDictionaryToClass();
        }
    }
}

namespace RedsSettings
{
    public class SettingsList
    {
        public string AdditionalHeadContent { get; internal set; }
        public string AdditionalMainDivContent { get; internal set; }

        public string HeaderElement { get; internal set; }
        public string FooterElement { get; internal set; }

        public string CurrentHtmlTextFormaterConfig { get; internal set; }

        public string CurrentThemeConfig { get; internal set; }
        public string CurrentTaskConfig { get; internal set; }
        public string CurrentResultConfig { get; internal set; }

        public string GlobalHtmlTextFormaterConfig { get; internal set; }

        public string ScriptsToUse { get; internal set; }
        public string StylesToUse { get; internal set; }

        public bool AddDebugDivToHtml { get; internal set; }

        public string ThemeCompileHashes { get; internal set; }


        public string ConfigFolderName { get; internal set; }
        public string DirsToScan { get; internal set; }
    } //Dev set

    public interface ISetting
    {
        SettingStr GetSettingStr();
        void SetFromString(string inputStrValue);
        void SetFromObject(object inputObject);
        string GetName();
        object GetValue();
    }

    internal class Setting<TValue> : ISetting
    {
        public TValue Value { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public void SetFromString(string inputStrValue)
        {
            Value = TConverter.ChangeType<TValue>(inputStrValue);
        }
        public void SetFromObject(object inputObject)
        {
            Value = (TValue)inputObject;
        }

        public object GetValue()
        {
            return Value;
        }
        public string GetName()
        {
            return Name;
        }

        public Setting(TValue value, string name, string description)
        {
            Value = value;
            Name = name;
            Description = description;
        }
        public SettingStr GetSettingStr()
        {
            SettingStr result = new SettingStr();

            result.Value = Value.ToString();
            result.Name = this.Name;
            result.Description = this.Description;

            return result;
        }
    }

    public class SettingStr
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public static class TConverter
    {
        public static T ChangeType<T>(object value)
        {
            return (T)ChangeType(typeof(T), value);
        }

        public static object ChangeType(Type t, object value)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(t);
            return tc.ConvertFrom(value);
        }

        public static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
        {
            TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
        }
    }
}