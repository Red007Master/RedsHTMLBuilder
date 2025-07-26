public class HtmlTextFormaterConfig
{
    public List<string> JsOpeningTags = new List<string>();
    public List<string> HtmlOpeningTags = new List<string>();
    public List<string> HighlightOpeningTags = new List<string>();

    public List<string> JsClosingTags = new List<string>();
    public List<string> HtmlClosingTags = new List<string>();
    public List<string> HighlightClosingTags = new List<string>();

    public List<string> JsTriggers = new List<string>();
    public List<string> HtmlTriggers = new List<string>();
    public List<string> HighlightTriggers = new List<string>();

    public string CSSClassJS = String.Empty;
    public string CSSClassHtml = String.Empty;
    public string CSSClassHighlight = String.Empty;

    public HtmlTextFormaterConfig()
    {

    }

    public HtmlTextFormaterConfig(List<string> jsClosingTags, List<string> htmlClosingTags, List<string> highlightClosingTags, List<string> jsOpeningTags, List<string> htmlOpeningTags, List<string> highlightOpeningTags, List<string> jsTriggers, List<string> htmlTriggers, List<string> highlightTriggers, string cSSClassJS, string cSSClassHTML, string cSSClassHighlight)
    {
        JsClosingTags = jsClosingTags;
        HtmlClosingTags = htmlClosingTags;
        HighlightClosingTags = highlightClosingTags;
        JsOpeningTags = jsOpeningTags;
        HtmlOpeningTags = htmlOpeningTags;
        HighlightOpeningTags = highlightOpeningTags;
        JsTriggers = jsTriggers;
        HtmlTriggers = htmlTriggers;
        HighlightTriggers = highlightTriggers;
        CSSClassJS = cSSClassJS;
        CSSClassHtml = cSSClassHTML;
        CSSClassHighlight = cSSClassHighlight;
    }

    internal void SetDefault()
    {
        JsOpeningTags.Add("*JS-*");
        HtmlOpeningTags.Add("*HT-*");
        HighlightOpeningTags.Add("*HI-*");

        JsClosingTags.Add("*-JS*");
        HtmlClosingTags.Add("*-HT*");
        HighlightClosingTags.Add("*-HI*");

        JsTriggers.Add("js");
        JsTriggers.Add("canvas");
        HtmlTriggers.Add("html");
        HighlightTriggers.Add("highlight");

        CSSClassJS = "attention-code";
        CSSClassHtml = "attention-code-second";
        CSSClassHighlight = "attention-highlight";
    }

    public void MargeWith(HtmlTextFormaterConfig htmlTextFormaterConfig)
    {
        JsOpeningTags.AddRange(htmlTextFormaterConfig.JsOpeningTags);
        HtmlOpeningTags.AddRange(htmlTextFormaterConfig.HtmlOpeningTags);
        HighlightOpeningTags.AddRange(htmlTextFormaterConfig.HighlightOpeningTags);

        JsClosingTags.AddRange(htmlTextFormaterConfig.JsClosingTags);
        HtmlClosingTags.AddRange(htmlTextFormaterConfig.HtmlClosingTags);
        HighlightClosingTags.AddRange(htmlTextFormaterConfig.HighlightClosingTags);

        JsTriggers.AddRange(htmlTextFormaterConfig.JsTriggers);
        HtmlTriggers.AddRange(htmlTextFormaterConfig.HtmlTriggers);
        HighlightTriggers.AddRange(htmlTextFormaterConfig.HighlightTriggers);

        JsOpeningTags = JsOpeningTags.Distinct().ToList();
        HtmlOpeningTags = HtmlOpeningTags.Distinct().ToList();
        HighlightOpeningTags = HighlightOpeningTags.Distinct().ToList();
        JsClosingTags = JsClosingTags.Distinct().ToList();
        HtmlClosingTags = HtmlClosingTags.Distinct().ToList();
        HighlightClosingTags = HighlightClosingTags.Distinct().ToList();
        JsTriggers = JsTriggers.Distinct().ToList();
        HtmlTriggers = HtmlTriggers.Distinct().ToList();
        HighlightTriggers = HighlightTriggers.Distinct().ToList();
    }
}
