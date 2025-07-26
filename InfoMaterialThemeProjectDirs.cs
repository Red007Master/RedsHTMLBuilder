using Newtonsoft.Json;

internal partial class Work
{
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

        public string CodefilesFolderPath { get; set; }

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

            CodefilesFolderPath = Path.Combine(corePath, "codefiles");

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
