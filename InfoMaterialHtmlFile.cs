using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RedsHTMLBuilder.Tools;

public class HtmlFile
{
    public HtmlDocument Document { get; set; }
    public HtmlNode HtmlNode { get; set; } = HtmlNode.CreateNode("<html></html>");
    public HtmlNode HeadNode { get; set; } = HtmlNode.CreateNode("<head></head>");
    public HtmlNode BodyNode { get; set; } = HtmlNode.CreateNode("<body></body>");

    public List<string> Styles { get; set; }
    public List<string> Scripts { get; set; }
    public List<string> AdditionalHeadContent { get; set; }

    public void Compile()
    {
        for (int i = 0; i < AdditionalHeadContent.Count; i++)
        {
            HtmlNode additionalHeadContentNode = HtmlNode.CreateNode(AdditionalHeadContent[i]);
            HeadNode.AppendChild(additionalHeadContentNode);
        }
        for (int i = 0; i < Styles.Count; i++)
        {
            HtmlNode stylesNode = HtmlNode.CreateNode($@"<link rel='stylesheet' href='{Styles[i]}'>");
            HeadNode.AppendChild(stylesNode);
        }
        for (int i = 0; i < Scripts.Count; i++)
        {
            HtmlNode scriptNode = HtmlNode.CreateNode($"<script src='{Scripts[i]}' defer></script>");
            HeadNode.AppendChild(scriptNode);
        }
    }


    public override string ToString()
    {
        CleanNewlines(Document.DocumentNode);

        return Document.DocumentNode.OuterHtml;
    }

    public void CleanNewlines(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Text && node.ParentNode.Name == "code")
        {
            // Do not remove newlines within <code> nodes
            return;
        }

        if (node.NodeType == HtmlNodeType.Text)
        {
            // Remove newlines within text nodes
            node.InnerHtml = node.InnerHtml.Replace("\n", "").Replace("\r", "");
        }

        foreach (var childNode in node.ChildNodes)
        {
            CleanNewlines(childNode);
        }
    }


    public HtmlFile()
    {
        Document = new HtmlDocument();

        Document.DocumentNode.AppendChild(HtmlNode);

        HtmlNode.AppendChild(HeadNode);
        HtmlNode.AppendChild(BodyNode);
    }
    public HtmlFile(List<string> styles = null, List<string> scripts = null, List<string> additionalHeadContent = null) : this()
    {
        Styles = styles ?? new List<string>();
        Scripts = scripts ?? new List<string>();
        AdditionalHeadContent = additionalHeadContent ?? new List<string>();
    }
}
public class InfoMaterialThemeHtmlFile : HtmlFile
{
    public string Title { get; set; }
    public string HeaderContent { get; set; }
    public string FooterContent { get; set; }
    public string Summary { get; set; } = "";

    public ThemesHashes ThemesHashes { get; set; }

    public HtmlNode HeaderNode { get; set; }
    public HtmlNode AdditionalMainDivContent { get; set; }
    public HtmlNode ContentNode { get; set; }
    public HtmlNode FooterNode { get; set; }

    public bool MainContainerIsEncrypted = false;

    public HtmlTextFormater HtmlTextFormater { get; set; }

    public List<InfoMaterialThemeMainContentContainerNode> InfoMaterialThemeMainContentContainerNodes = new List<InfoMaterialThemeMainContentContainerNode>();

    public void Compile()
    {
        HeadNode.AppendChild(HtmlNode.CreateNode($"<title>{Title}</title>"));

        if (HeaderNode != null)
        {
            HtmlNode old = HeaderNode.SelectSingleNode("//a[@class='themeA']");
            HtmlNode tmp = HtmlNode.CreateNode($"<a href='#' class='themeA'>{HeaderContent}</a>");

            old.ParentNode.ReplaceChild(tmp, old);

            BodyNode.AppendChild(HeaderNode);
        }

        ContentNode = HtmlNode.CreateNode($"<div class='content'></div>");

        if (P.Settings.SettingsList.AddDebugDivToHtml)
        {
            using (TimeLogger tl = new TimeLogger("Debug mode is on, writing CompilationInfoNode", LogLevel.Information, P.Logger, 4))
            {
                ContentNode.AppendChild(GetCompilationInfoNode());
            }
        }
        else
        {
            P.Logger.Log("Debug mode is off, continuing without CompilationInfoNode", LogLevel.Information, 4);
        }

        if (AdditionalMainDivContent != null)
        {
            ContentNode.AppendChild(AdditionalMainDivContent);
        }

        if (Summary.Length > 0)
        {
            ContentNode.AppendChild(HtmlNode.CreateNode($"<div class='theme-main-container' align='center'><p class='theme-main-header'>{Summary}</p></div>"));
        }


        for (int i = 0; i < InfoMaterialThemeMainContentContainerNodes.Count; i++)
        {
            InfoMaterialThemeMainContentContainerNodes[i].Complie();
            ContentNode.AppendChild(InfoMaterialThemeMainContentContainerNodes[i].Core);
        }

        //there to encode code div content

        if (MainContainerIsEncrypted)
        {
            Scripts.Insert(0, "../../../scripts/decrypt_dialogue.js");

            string encryptedContentDiv = StringsTools.EncryptString(ContentNode.OuterHtml, P.Settings.SettingsList.ResultEncryptionKey);

            HtmlNode payloadCarier = HtmlNode.CreateNode($"<div class='encrypted-content-node' id='encrypted-content-node'>{encryptedContentDiv}</div>");

            BodyNode.AppendChild(payloadCarier);
        }
        else
        {
            BodyNode.AppendChild(ContentNode);
        }

        //TODO

        if (FooterNode != null)
        {
            HtmlNode old = FooterNode.SelectSingleNode("//div[@id='templatemo_footer']");
            HtmlNode tmp = HtmlNode.CreateNode($"<div id='templatemo_footer'>{FooterContent}</div>");

            old.ParentNode.ReplaceChild(tmp, old);

            BodyNode.AppendChild(FooterNode);
        }

        base.Compile();
    }

    private HtmlNode GetCompilationInfoNode()
    {
        HtmlNode result = HtmlNode.CreateNode("<div class='theme-main-container debug-compile-info-cont' align='center'></div>");

        result.AppendChild(HtmlNode.CreateNode("<p class='theme-main-header debug-compile-info-header'>Complie info</p>"));

        result.AppendChild(GetCompileNameValueNode("ExecutableCreationTime", P.ExecutableCreationTime));
        result.AppendChild(GetCompileNameValueNode("ExecutableLastWriteTime", P.ExecutableLastWriteTime));
        result.AppendChild(GetCompileNameValueNode("ExecutableHashSHA256", P.ExecutableHashSHA256));
        result.AppendChild(GetCompileNameValueNode("SettingsHashSHA256", P.SettingsHashSHA256));

        result.AppendChild(HtmlNode.CreateNode("<br>"));

        result.AppendChild(GetCompileNameValueNode("AppStartDate", P.AppStartDate));

        result.AppendChild(HtmlNode.CreateNode("<br>"));

        result.AppendChild(GetCompileNameValueNode("StylesHash", ThemesHashes.StylesHash));
        result.AppendChild(GetCompileNameValueNode("ScriptsHash", ThemesHashes.ScriptsHash));
        result.AppendChild(GetCompileNameValueNode("AdditionalHeadContentHash", ThemesHashes.AdditionalHeadContentHash));

        result.AppendChild(HtmlNode.CreateNode("<br>"));

        result.AppendChild(GetCompileNameValueNode("InfoMaterialThemeConfigStringHash", ThemesHashes.InfoMaterialThemeConfigStringHash));
        result.AppendChild(GetCompileNameValueNode("HtmlTextFormaterStringHash", ThemesHashes.HtmlTextFormaterStringHash));
        result.AppendChild(GetCompileNameValueNode("GlobalHtmlTextFormaterStringHash", ThemesHashes.GlobalHtmlTextFormaterStringHash));
        result.AppendChild(GetCompileNameValueNode("HeaderHash", ThemesHashes.HeaderHash));
        result.AppendChild(GetCompileNameValueNode("FooterHash", ThemesHashes.FooterHash));
        result.AppendChild(GetCompileNameValueNode("AdditionalMainDivContentHash", ThemesHashes.AdditionalMainDivContentHash));

        return result;
    }

    private HtmlNode GetCompileNameValueNode(string name, string value)
    {
        return HtmlNode.CreateNode($"<p class='theme-main-header debug-compile-info-entry'><span class='debug-compile-info-entry-name'>{name}</span>:<br><span class='debug-compile-info-entry-value attention-code'>{value}</span></p>");
    }

    public InfoMaterialThemeHtmlFile(List<string> styles = null, List<string> scripts = null, List<string> additionalHeadContent = null) : base(styles, scripts, additionalHeadContent)
    {
    }

    public InfoMaterialThemeHtmlFile() { }

    public InfoMaterialThemeHtmlFile(List<string> styles, List<string> scripts, List<string> additionalHeadContent, string infoMaterialThemeConfigString, string htmlTextFormaterString, string globalHtmlTextFormaterString, string header, string footer, string additionalMainDivContent, int themeNumber) : base(styles, scripts, additionalHeadContent)
    {
        HtmlTextFormater = new HtmlTextFormater(JsonConvert.DeserializeObject<HtmlTextFormaterConfig>(htmlTextFormaterString));
        HtmlTextFormater.AppendConfig(JsonConvert.DeserializeObject<HtmlTextFormaterConfig>(globalHtmlTextFormaterString));

        InfoMaterialThemeConfig infoMaterialThemeConfig = JsonConvert.DeserializeObject<InfoMaterialThemeConfig>(infoMaterialThemeConfigString);

        Title = infoMaterialThemeConfig.Title.Replace("{ThemeNumber}", Convert.ToString(themeNumber));
        HeaderContent = infoMaterialThemeConfig.HeaderContent.Replace("{ThemeNumber}", Convert.ToString(themeNumber));
        FooterContent = infoMaterialThemeConfig.FooterContent;
        Summary = infoMaterialThemeConfig.Summary;

        if (additionalMainDivContent.Length > 0)
        {
            AdditionalMainDivContent = HtmlNode.CreateNode(additionalMainDivContent);
        }
        else
        {
            AdditionalMainDivContent = null;
        }

        HeaderNode = HtmlNode.CreateNode(header);
        FooterNode = HtmlNode.CreateNode(footer);

        string other = Convert.ToString(themeNumber);
        ThemesHashes = new ThemesHashes(styles, scripts, additionalHeadContent, infoMaterialThemeConfigString, htmlTextFormaterString, globalHtmlTextFormaterString, header, footer, additionalMainDivContent, P.ExecutableHashSHA256, other);

        InfoMaterialThemeMainContentContainerNodes = GenerateInfoMaterialThemeMainContentContainerNodes(infoMaterialThemeConfig.InfoMaterialThemeMainContentContainerConfigs, infoMaterialThemeConfig.Title);
    }

    private List<InfoMaterialThemeMainContentContainerNode> GenerateInfoMaterialThemeMainContentContainerNodes(List<InfoMaterialThemeMainContentContainerConfig> infoMaterialThemeMainContentContainerConfigs, string indicator)
    {
        List<InfoMaterialThemeMainContentContainerNode> result = new List<InfoMaterialThemeMainContentContainerNode>();

        for (int i = 0; i < infoMaterialThemeMainContentContainerConfigs.Count; i++)
        {
            // InfoMaterialThemeMainContentContainerNode temp = new InfoMaterialThemeMainContentContainerNode
            //     (infoMaterialThemeMainContentContainerConfigs[i].Header, infoMaterialThemeMainContentContainerConfigs[i].TextContainerInfo, HtmlTextFormater);

            InfoMaterialThemeMainContentContainerNode temp = new InfoMaterialThemeMainContentContainerNode(infoMaterialThemeMainContentContainerConfigs[i], HtmlTextFormater);

            temp.CodeContainerNodes = GenerateCodeContainerNodes(infoMaterialThemeMainContentContainerConfigs[i].InfoMaterialThemeCodeAndExplanationContainerConfigs, $"[{indicator}] in [{infoMaterialThemeMainContentContainerConfigs[i].Header}]");

            result.Add(temp);
        }

        return result;
    }

    private List<InfoMaterialThemeCodeAndExplanationContainerNode> GenerateCodeContainerNodes(List<InfoMaterialThemeCodeAndExplanationContainerConfig> infoMaterialThemeCodeAndExplanationContainerConfigs, string indicator)
    {
        List<InfoMaterialThemeCodeAndExplanationContainerNode> result = new List<InfoMaterialThemeCodeAndExplanationContainerNode>();

        for (int i = 0; i < infoMaterialThemeCodeAndExplanationContainerConfigs.Count; i++)
        {
            InfoMaterialThemeCodeAndExplanationContainerNode temp = new InfoMaterialThemeCodeAndExplanationContainerNode();
            temp.Elems = GenerateElems(infoMaterialThemeCodeAndExplanationContainerConfigs[i].AdditionalContentElemConfigs, indicator);
            result.Add(temp);
        }

        return result;
    }

    private List<AdditionalContentElemNodeCore> GenerateElems(List<AdditionalContentElemConfig> additionalContentElemConfigs, string indicator)
    {
        List<AdditionalContentElemNodeCore> result = new List<AdditionalContentElemNodeCore>();

        int totalCount = 0;
        int codeCount = 0;

        for (int i = 0; i < additionalContentElemConfigs.Count; i++)
        {
            totalCount++;

            if (additionalContentElemConfigs[i].Text.Length > 0)
            {
                result.Add(new AdditionalContentTextNode(additionalContentElemConfigs[i].Text, additionalContentElemConfigs[i].Format, HtmlTextFormater));
            }
            else if (additionalContentElemConfigs[i].Code.Length > 0)
            {
                codeCount++;
                result.Add(new AdditionalContentCodeNode(additionalContentElemConfigs[i].Code, additionalContentElemConfigs[i].CodeTitle, additionalContentElemConfigs[i].LanguageClass, $"In [{indicator}], totalCount=[{totalCount}], codeCount=[{codeCount}]"));


            }
            else if (additionalContentElemConfigs[i].Src.Length > 0)
            {
                if (additionalContentElemConfigs[i].Src.EndsWith(".mp4") || additionalContentElemConfigs[i].Src.EndsWith(".mkv"))
                {
                    result.Add(new AdditionalContentVideoNode(additionalContentElemConfigs[i].Src));
                }
                else
                {
                    result.Add(new AdditionalContentImgNode(additionalContentElemConfigs[i].Src));
                }
            }
            else if (additionalContentElemConfigs[i].OtherHtml.Length > 0)
            {
                result.Add(new AdditionalContentOtherHtmlNode(additionalContentElemConfigs[i].OtherHtml));
            }
        }

        return result;
    }
}

public class InfoMaterialThemeMainContentContainerNode
{
    public InfoMaterialThemeMainContentContainerConfig Config { get; set; }
    public HtmlTextFormater HtmlTextFormater { get; set; }

    public List<InfoMaterialThemeCodeAndExplanationContainerNode> CodeContainerNodes { get; set; } = new List<InfoMaterialThemeCodeAndExplanationContainerNode>();

    public HtmlNode Core { get; set; } = HtmlNode.CreateNode("<div class='theme-main-container' align='center'></div>"); //MainContainer
    public HtmlNode AdditionalContentContainerMain { get; set; } = HtmlNode.CreateNode("<div class='theme-additional-content-container-main'></div>");

    // public InfoMaterialThemeMainContentContainerNode(string header, string textContainerInfo, HtmlTextFormater htmlTextFormater)
    // {
    //     Header = header;
    //     TextContainerInfo = textContainerInfo;
    //     HtmlTextFormater = htmlTextFormater;
    // }

    public InfoMaterialThemeMainContentContainerNode(InfoMaterialThemeMainContentContainerConfig config, HtmlTextFormater htmlTextFormater)
    {
        Config = config;
        HtmlTextFormater = htmlTextFormater;
    }

    public void Complie()
    {
        if (Config.Header != null)
        {
            if (Config.FormatHeader)
            {
                Core.AppendChild(HtmlNode.CreateNode($"<h1 class='theme-header'>{HtmlTextFormater.Format(Config.Header)}</h1>"));
            }
            else
            {
                Core.AppendChild(HtmlNode.CreateNode($"<h1 class='theme-header'>{Config.Header}</h1>"));
            }
        }

        if (Config.TextContainerInfo != null)
        {
            if (Config.FormetTextContainerInfo)
            {
                Core.AppendChild(HtmlNode.CreateNode($"<p class='theme-text-container theme-text-container-info'>{HtmlTextFormater.Format(Config.TextContainerInfo)}</p>"));
            }
            else
            {
                Core.AppendChild(HtmlNode.CreateNode($"<p class='theme-text-container theme-text-container-info'>{Config.TextContainerInfo}</p>"));
            }
        }

        if (CodeContainerNodes.Count > 0)
        {
            Core.AppendChild(AdditionalContentContainerMain);
            AdditionalContentContainerMain.AppendChild(HtmlNode.CreateNode("<button class='button-main theme-additional-content-expander'>Показати</button>"));
            var addContentCont = HtmlNode.CreateNode("<div class='theme-additional-content-container collapsed'></div>");

            for (int i = 0; i < CodeContainerNodes.Count; i++)
            {
                CodeContainerNodes[i].Compile();

                addContentCont.AppendChild(CodeContainerNodes[i].Core);
            }
            AdditionalContentContainerMain.AppendChild(addContentCont);
        }
    }
}

public class InfoMaterialThemeCodeAndExplanationContainerNode
{
    public HtmlNode Core { get; set; } = HtmlNode.CreateNode("<div class='code-and-explanation-container'></div>"); //code-and-explanation-container
    public List<AdditionalContentElemNodeCore> Elems = new List<AdditionalContentElemNodeCore>();

    public void Compile()
    {
        for (int i = 0; i < Elems.Count; i++)
        {
            Elems[i].Compile();
            Core.AppendChild(Elems[i].Core);
        }
    }
}

public class AdditionalContentElemNodeCore
{
    public HtmlNode Core { get; set; }

    public virtual void Compile() { }
}
public class AdditionalContentTextNode : AdditionalContentElemNodeCore
{
    public string Text { get; set; }
    public bool Format { get; set; } = true;
    public HtmlTextFormater HtmlTextFormater { get; set; }

    public AdditionalContentTextNode(string text, bool format, HtmlTextFormater htmlTextFormater)
    {
        Text = text;
        Format = format;

        HtmlTextFormater = htmlTextFormater;
    }

    public override void Compile()
    {
        if (Format)
        {
            Core = HtmlNode.CreateNode($"<p class='theme-text-container theme-text-container-code-comment'>{HtmlTextFormater.Format(Text)}</p>");
        }
        else
        {
            Core = HtmlNode.CreateNode($"<p class='theme-text-container theme-text-container-code-comment'>{Text}</p>");
        }
    }
}
public class AdditionalContentCodeNode : AdditionalContentElemNodeCore
{
    public string Code { get; set; }
    public string CodeTitle { get; set; }
    public string LanguageClass { get; set; }
    public string PathFindersIndicator { get; set; }


    public void TryCodeCompile(string code, string languageClass, string pathFindersIndicator)
    {
        if (P.ComplieCSharpCode && languageClass == "language-cs" || languageClass == "language-csharp")
        {
            string codeToWorkWith = System.Net.WebUtility.HtmlDecode(code);
            string mainMethodError = "Program does not contain a static 'Main' method suitable for an entry point".ToLower();

            using (TimeLogger tl = new TimeLogger($"Building [{pathFindersIndicator}]", LogLevel.Information, P.Logger, 4))
            {
                CompilationResult result = CodeCompiler.CompileCode(codeToWorkWith, P.PathDirs.SharpBuild);

                if (result.Success)
                {
                    P.Logger.Log("Build - Success!", LogLevel.Information, 5);
                }
                else if (!result.Success)
                {
                    if (result.Message.ToLower().Contains(mainMethodError))
                    {
                        P.Logger.Log("Build - NO MAIN!", LogLevel.Warning, 5);
                    }
                    else
                    {
                        P.Logger.Log($"Build - FAILED! MSG=[{result.Message}]", LogLevel.Error, 5);
                        P.Logger.Log($"Code:\n\n------------------------------\n{codeToWorkWith}\n------------------------------\n\n", LogLevel.Error, 6);
                    }
                }
            }
        }
    }

    public AdditionalContentCodeNode(string code, string codeTitle, string languageClass, string pathFindersIndicator)
    {
        Code = code;
        CodeTitle = codeTitle;
        LanguageClass = languageClass;
        PathFindersIndicator = pathFindersIndicator;
    }

    public override void Compile()
    {
        TryCodeCompile(Code, LanguageClass, PathFindersIndicator);
        if (LanguageClass == null || LanguageClass.Length <= 0)
        {
            LanguageClass = "language-js";
        }

        if (CodeTitle == "")
        {
            Core = HtmlNode.CreateNode($"<div class='code-pre-container'><pre><code class='{LanguageClass}'>{Code}</code></pre></div>");
        }
        else
        {
            Core = HtmlNode.CreateNode($"<div class='code-pre-container'><p class='code-pre-title'>{CodeTitle}:</p><pre><code class='{LanguageClass}'>{Code}</code></pre></div>");
        }

    }
}
public class AdditionalContentImgNode : AdditionalContentElemNodeCore
{
    public string Src { get; set; }

    public AdditionalContentImgNode(string src)
    {
        Src = src;
    }

    public override void Compile()
    {
        HtmlNode otherHtmlContainerNode = HtmlNode.CreateNode("<div class='theme-additional-html-content-container'></div>");
        otherHtmlContainerNode.AppendChild(HtmlNode.CreateNode($"<img src='{Src}' class='theme-additional-img' alt=''>"));
        Core = otherHtmlContainerNode;
    }
}
public class AdditionalContentVideoNode : AdditionalContentElemNodeCore
{
    public string Src { get; set; }

    public AdditionalContentVideoNode(string src)
    {
        Src = src;
    }

    public override void Compile()
    {
        HtmlNode otherHtmlContainerNode = HtmlNode.CreateNode("<div class='theme-additional-html-content-container'></div>");
        otherHtmlContainerNode.AppendChild(HtmlNode.CreateNode($"<video class='theme-additional-img' controls><source src='{Src}'></video>"));
        Core = otherHtmlContainerNode;
    }
}
public class AdditionalContentOtherHtmlNode : AdditionalContentElemNodeCore
{
    public string Html { get; set; }

    public AdditionalContentOtherHtmlNode(string html)
    {
        Html = html;
    }

    public override void Compile()
    {
        HtmlNode otherHtmlContainerNode = HtmlNode.CreateNode("<div class='theme-additional-html-content-container'></div>");
        otherHtmlContainerNode.AppendChild(HtmlNode.CreateNode(Html));
        Core = otherHtmlContainerNode;
    }
}

public class InfoMaterialThemeConfig
{
    public InfoMaterialThemeConfig()
    {
    }

    public InfoMaterialThemeConfig(InfoMaterialThemeConfigType dummyType)
    {
        //Title = "dummyTitle";
        Title = "Тема {ThemeNumber}";

        //HeaderContent = "headerDummy";
        HeaderContent = "Тема {ThemeNumber}<br>XXXXX";

        //FooterContent = "footerDummy";
        FooterContent = "Copyright © 2024 WebSkill - Всі права захищено.";

        InfoMaterialThemeMainContentContainerConfigs = new List<InfoMaterialThemeMainContentContainerConfig>();

        if (dummyType == InfoMaterialThemeConfigType.Theme)
        {
            Summary = "summaryDummy";
            InfoMaterialThemeMainContentContainerConfigs.Add(new InfoMaterialThemeMainContentContainerConfig(dummyType));
        }
        else if (dummyType == InfoMaterialThemeConfigType.Tasks)
        {
            Summary = "<h1 class='theme-header result-page-announcer'>Завдання</h1>";
            InfoMaterialThemeMainContentContainerConfigs.Add(new InfoMaterialThemeMainContentContainerConfig(dummyType));
        }
        else if (dummyType == InfoMaterialThemeConfigType.Results)
        {
            Summary = "<h1 class='theme-header result-page-announcer'>Відповіді на завдання</h1>";
            InfoMaterialThemeMainContentContainerConfigs.Add(new InfoMaterialThemeMainContentContainerConfig(dummyType));
        }

    }

    public InfoMaterialThemeConfig(string title, string headerContent, string footerContent, string summary, List<InfoMaterialThemeMainContentContainerConfig> infoMaterialThemeMainContentContainerConfigs)
    {
        Title = title;
        HeaderContent = headerContent;
        FooterContent = footerContent;
        Summary = summary;
        InfoMaterialThemeMainContentContainerConfigs = infoMaterialThemeMainContentContainerConfigs;
    }

    public string Title { get; set; }
    public string HeaderContent { get; set; }
    public string FooterContent { get; set; }
    public string Summary { get; set; } = "";

    public List<InfoMaterialThemeMainContentContainerConfig> InfoMaterialThemeMainContentContainerConfigs { get; set; } = new List<InfoMaterialThemeMainContentContainerConfig>();
}

public enum InfoMaterialThemeConfigType
{
    Theme = 0,
    Tasks = 1,
    Results = 2
}

public class InfoMaterialThemeMainContentContainerConfig
{
    public InfoMaterialThemeMainContentContainerConfig()
    {
    }

    public InfoMaterialThemeMainContentContainerConfig(InfoMaterialThemeConfigType dummyType)
    {
        InfoMaterialThemeCodeAndExplanationContainerConfigs = new List<InfoMaterialThemeCodeAndExplanationContainerConfig>();

        List<AdditionalContentElemConfig> additionalContentElemConfigs = new List<AdditionalContentElemConfig>();

        additionalContentElemConfigs.Add(new AdditionalContentElemConfig("dummyAdditionalContentElemConfigText"));

        InfoMaterialThemeCodeAndExplanationContainerConfigs.Add(new InfoMaterialThemeCodeAndExplanationContainerConfig());

        if (dummyType == InfoMaterialThemeConfigType.Theme)
        {
            Header = "themeDummyText";
            TextContainerInfo = "themeDummyTextThemeThisIsAboutDummyText";
        }
        else if (dummyType == InfoMaterialThemeConfigType.Tasks)
        {
            Header = "Завдання XX.XX";
            TextContainerInfo = "themeDummyTextThemeThisIsAboutDummyText";

        }
        else if (dummyType == InfoMaterialThemeConfigType.Results)
        {
            Header = "Завдання XX.XX";
            TextContainerInfo = "Результат";

        }
    }

    public InfoMaterialThemeMainContentContainerConfig(string header, string textContainerInfo, List<InfoMaterialThemeCodeAndExplanationContainerConfig> infoMaterialThemeCodeAndExplanationContainerConfigs)
    {
        Header = header;
        TextContainerInfo = textContainerInfo;
        InfoMaterialThemeCodeAndExplanationContainerConfigs = infoMaterialThemeCodeAndExplanationContainerConfigs;
    }


    public string Header { get; set; }
    public bool FormatHeader { get; set; } = true;

    public string TextContainerInfo { get; set; }
    public bool FormetTextContainerInfo { get; set; } = true;

    public List<InfoMaterialThemeCodeAndExplanationContainerConfig> InfoMaterialThemeCodeAndExplanationContainerConfigs { get; set; } = new List<InfoMaterialThemeCodeAndExplanationContainerConfig>();
}
public class InfoMaterialThemeCodeAndExplanationContainerConfig
{
    public InfoMaterialThemeCodeAndExplanationContainerConfig()
    {
    }

    public InfoMaterialThemeCodeAndExplanationContainerConfig(List<AdditionalContentElemConfig> additionalContentElemConfigs)
    {
        AdditionalContentElemConfigs = additionalContentElemConfigs;
    }

    public List<AdditionalContentElemConfig> AdditionalContentElemConfigs { get; set; } = new List<AdditionalContentElemConfig>();
}
public class AdditionalContentElemConfig
{
    public string Text { get; set; } = "";
    public string Code { get; set; } = "";
    public string CodeTitle { get; set; } = "";
    public string Src { get; set; } = "";
    public string OtherHtml { get; set; } = "";
    public string LanguageClass { get; set; }
    public bool Format { get; set; } = true;

    public AdditionalContentElemConfig()
    {
    }

    public AdditionalContentElemConfig(string text)
    {
        Text = text;
    }
    public AdditionalContentElemConfig(string code, string codeTitle, string languageClass)
    {
        Code = code;
        CodeTitle = codeTitle;
        LanguageClass = languageClass;
    }
}

public class Whome
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Path { get; set; } = "";
    public int ThemeNumber { get; set; } = -1;

    public Whome(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public Whome(InfoMaterialThemeHtmlFile infoMaterialThemeHtmlFile, int themeNumber, string path)
    {
        Title = infoMaterialThemeHtmlFile.Title;
        Description = infoMaterialThemeHtmlFile.HeaderContent;
        ThemeNumber = themeNumber;
        Path = path;
    }
}

public class WhomeComparer : IComparer<Whome>
{
    public int Compare(Whome x, Whome y)
    {
        return x.ThemeNumber.CompareTo(y.ThemeNumber);
    }
}

public class CourseWhome{
    public string CourseTitle {get; set;} = String.Empty;

    public List<Whome> Whomes {get; set; } = new List<Whome>();

    internal void Sort()
    {
        Whomes.Sort(new WhomeComparer());
    }
}

public class ThemesHashes
{
    public string StylesHash = String.Empty;
    public string ScriptsHash = String.Empty;
    public string AdditionalHeadContentHash = String.Empty;

    public string InfoMaterialThemeConfigStringHash = String.Empty;
    public string HtmlTextFormaterStringHash = String.Empty;
    public string GlobalHtmlTextFormaterStringHash = String.Empty;
    public string HeaderHash = String.Empty;
    public string FooterHash = String.Empty;
    public string AdditionalMainDivContentHash = String.Empty;

    public string OtherHash = String.Empty;

    public string ExecutableHashSHA256 = String.Empty;

    public ThemesHashes(List<string> styles, List<string> scripts, List<string> additionalHeadContent, string infoMaterialThemeConfigString, string htmlTextFormaterString, string globalHtmlTextFormaterString, string header, string footer, string additionalMainDivContent, string executableHashSHA256, string other)
    {
        StylesHash = Hash.GetStringSHA256(String.Join(String.Empty, styles.ToArray()));
        ScriptsHash = Hash.GetStringSHA256(String.Join(String.Empty, scripts.ToArray()));
        AdditionalHeadContentHash = Hash.GetStringSHA256(String.Join(String.Empty, additionalHeadContent.ToArray()));

        InfoMaterialThemeConfigStringHash = Hash.GetStringSHA256(infoMaterialThemeConfigString);
        HtmlTextFormaterStringHash = Hash.GetStringSHA256(htmlTextFormaterString);
        GlobalHtmlTextFormaterStringHash = Hash.GetStringSHA256(globalHtmlTextFormaterString);
        HeaderHash = Hash.GetStringSHA256(header);
        FooterHash = Hash.GetStringSHA256(footer);
        AdditionalMainDivContentHash = Hash.GetStringSHA256(additionalMainDivContent);

        OtherHash = Hash.GetStringSHA256(other);

        ExecutableHashSHA256 = executableHashSHA256;
    }

    public ThemesHashes() { }

    public static bool operator ==(ThemesHashes obj1, ThemesHashes obj2)
    {
        if (ReferenceEquals(obj1, obj2))
        {
            return true;
        }

        if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
        {
            return false;
        }

        return obj1.StylesHash == obj2.StylesHash &&
               obj1.ScriptsHash == obj2.ScriptsHash &&
               obj1.AdditionalHeadContentHash == obj2.AdditionalHeadContentHash &&
               obj1.InfoMaterialThemeConfigStringHash == obj2.InfoMaterialThemeConfigStringHash &&
               obj1.HtmlTextFormaterStringHash == obj2.HtmlTextFormaterStringHash &&
               obj1.GlobalHtmlTextFormaterStringHash == obj2.GlobalHtmlTextFormaterStringHash &&
               obj1.HeaderHash == obj2.HeaderHash &&
               obj1.FooterHash == obj2.FooterHash &&
               obj1.ExecutableHashSHA256 == obj2.ExecutableHashSHA256 &&
               obj1.OtherHash == obj2.OtherHash &&
               obj1.AdditionalMainDivContentHash == obj2.AdditionalMainDivContentHash;
    }
    public static bool operator !=(ThemesHashes obj1, ThemesHashes obj2)
    {
        return !(obj1 == obj2);
    }
}
