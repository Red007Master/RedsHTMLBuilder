using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class HtmlTextFormater
{
    private HtmlTextFormaterConfig HtmlTextFormaterConfig { get; set; }

    public string Format(string input)
    {
        string result = input;

        if (input == null)
        {
            P.Logger.Log("input == null, digly digly hole", LogLevel.FatalError);
        }

        {
            List<List<string>> otherOpeningTags = new List<List<string>>();
            otherOpeningTags.Add(HtmlTextFormaterConfig.HtmlOpeningTags);
            otherOpeningTags.Add(HtmlTextFormaterConfig.JsOpeningTags);

            List<List<string>> otherClosingTags = new List<List<string>>();
            otherClosingTags.Add(HtmlTextFormaterConfig.HtmlClosingTags);
            otherClosingTags.Add(HtmlTextFormaterConfig.JsClosingTags);

            //result = ParamFormat(result, HtmlTextFormaterConfig.HighlightOpeningTags, HtmlTextFormaterConfig.HighlightClosingTags, HtmlTextFormaterConfig.HighlightTriggers, HtmlTextFormaterConfig.CSSClassHighlight, otherOpeningTags, otherClosingTags);
            result = NewParamFormat(result, HtmlTextFormaterConfig.HighlightOpeningTags, HtmlTextFormaterConfig.HighlightClosingTags, HtmlTextFormaterConfig.HighlightTriggers, HtmlTextFormaterConfig.CSSClassHighlight);
        }


        {
            List<List<string>> otherOpeningTags = new List<List<string>>();
            otherOpeningTags.Add(HtmlTextFormaterConfig.HtmlOpeningTags);
            otherOpeningTags.Add(HtmlTextFormaterConfig.HighlightOpeningTags);

            List<List<string>> otherClosingTags = new List<List<string>>();
            otherClosingTags.Add(HtmlTextFormaterConfig.HtmlClosingTags);
            otherClosingTags.Add(HtmlTextFormaterConfig.HighlightClosingTags);

            //result = ParamFormat(result, HtmlTextFormaterConfig.JsOpeningTags, HtmlTextFormaterConfig.JsClosingTags, HtmlTextFormaterConfig.JsTriggers, HtmlTextFormaterConfig.CSSClassJS, otherOpeningTags, otherClosingTags);
            result = NewParamFormat(result, HtmlTextFormaterConfig.JsOpeningTags, HtmlTextFormaterConfig.JsClosingTags, HtmlTextFormaterConfig.JsTriggers, HtmlTextFormaterConfig.CSSClassJS);
        }


        {
            List<List<string>> otherOpeningTags = new List<List<string>>();
            otherOpeningTags.Add(HtmlTextFormaterConfig.JsOpeningTags);
            otherOpeningTags.Add(HtmlTextFormaterConfig.HighlightOpeningTags);

            List<List<string>> otherClosingTags = new List<List<string>>();
            otherClosingTags.Add(HtmlTextFormaterConfig.JsClosingTags);
            otherClosingTags.Add(HtmlTextFormaterConfig.HighlightClosingTags);

            //result = ParamFormat(result, HtmlTextFormaterConfig.HtmlOpeningTags, HtmlTextFormaterConfig.HtmlClosingTags, HtmlTextFormaterConfig.HtmlTriggers, HtmlTextFormaterConfig.CSSClassHtml, otherOpeningTags, otherClosingTags);
            result = NewParamFormat(result, HtmlTextFormaterConfig.HtmlOpeningTags, HtmlTextFormaterConfig.HtmlClosingTags, HtmlTextFormaterConfig.HtmlTriggers, HtmlTextFormaterConfig.CSSClassHtml);
        }

        return result;
    }

    private string NewParamFormat(string input, List<string> openingTags, List<string> closingTags, List<string> triggers, string cssClass)
    {
        string result = input;

        //if (triggers.Contains("highlight".ToLower()))
        //{
        //    Console.WriteLine();
        //}

        string openingTag = "dq45w34i3o6dq3i4ow35d3h3o3qiw335hdi6oqhwidhqwodqwjxioqpw8xuqwxhq".ToLower();
        string closingTag = "dqd123e23rio4h5juh756j89kjkhj23uih2uigh3u42uih2uih4ui2h36jn7ui4n".ToLower();

        for (int i = 0; i < openingTags.Count; i++)
            result = result.Replace(openingTags[i], openingTag);

        for (int i = 0; i < closingTags.Count; i++)
            result = result.Replace(closingTags[i], closingTag);

        result = FormatStringsOutsideTags(result, triggers.ToArray(), openingTag, closingTag);

        result = result.Replace(openingTag, CreateOpenSpan(cssClass));
        result = result.Replace(closingTag, "</span>");

        return result;
    }

    static string FormatStringsOutsideTags(string inputText, string[] stringsToFormat, string openingString, string closingString)
    {
        //string pattern = $@"(?i)(?<!\.)\b(?:{string.Join("|", Array.ConvertAll(stringsToFormat, Regex.Escape))})(?=\W|$)";

        string escapedStringsPattern = string.Join("|", Array.ConvertAll(stringsToFormat, Regex.Escape));
        string pattern = $@"(?i)(?<!\w)(?<!\.)({escapedStringsPattern})(?!\w)";

        string formattedText = Regex.Replace(inputText, pattern, match =>
        {
            // Check if the match is not within a tag
            if (!IsInsideTag(inputText, match.Index))
            {
                return $"{openingString}{match.Value}{closingString}";
            }
            return match.Value;
        });

        return formattedText;
    }


    static bool IsInsideTag(string text, int index)
    {
        int openTagIndex = text.LastIndexOf('<', index);
        int closeTagIndex = text.LastIndexOf('>', index);

        return openTagIndex > closeTagIndex;
    }

    private string ParamFormat(string input, List<string> openingTags, List<string> closingTags, List<string> triggers, string cssClass, List<List<string>> otherOpeningTags, List<List<string>> otherClosingTags)
    {
        string result = input;

        //if (triggers.Contains("highlight".ToLower()))
        //{
        //    Console.WriteLine();
        //}

        string openingTag = "|>-OPEN->TMP_OPEN_TAG<-OPEN-<|".ToLower();
        string closingTag = "|>-CLOSED->TMP_CLOSED_TAG<-CLOSED-<|".ToLower();

        for (int i = 0; i < openingTags.Count; i++)
            result = result.Replace(openingTags[i], openingTag);

        for (int i = 0; i < closingTags.Count; i++)
            result = result.Replace(closingTags[i], closingTag);

        string[] words = result.Split(' ');

        for (int x = 0; x < words.Length; x++)
        {
            for (int y = 0; y < triggers.Count; y++)
            {
                string word = words[x].ToLower();
                string trigger = triggers[y].ToLower();

                //if (word.Contains("by") && trigger == "y")
                //{
                //    Console.WriteLine();
                //}

                int indexOfTrigger = word.IndexOf(trigger);

                if (indexOfTrigger >= 0)
                {
                    bool triggerIsValid = false;

                    bool isCharBeforeTrigerIsInvalid = true;
                    bool isLetterIsAfterTrigger = true;

                    bool isOnZeroIndex = indexOfTrigger == 0;
                    bool isStringEndsOnTrigger = indexOfTrigger + trigger.Length == word.Length;

                    if (!isOnZeroIndex)
                    {
                        char tmpChar = word[indexOfTrigger - 1];
                        isCharBeforeTrigerIsInvalid = Char.IsLetter(tmpChar) || tmpChar == '<' || tmpChar == '-' || tmpChar == ';';
                    }
                    else
                    {
                        isCharBeforeTrigerIsInvalid = false;
                    }


                    if (!isStringEndsOnTrigger)
                    {
                        char tmpChar = word[indexOfTrigger + trigger.Length];
                        isLetterIsAfterTrigger = Char.IsLetter(tmpChar);
                    }
                    else
                    {
                        isLetterIsAfterTrigger = false;
                    }

                    if (!isCharBeforeTrigerIsInvalid && !isLetterIsAfterTrigger)
                    {
                        triggerIsValid = true;
                    }
                    else if (isOnZeroIndex && isStringEndsOnTrigger)
                    {
                        triggerIsValid = true;
                    }

                    if (triggerIsValid)
                    {
                        bool containsOpeningTag = word.IndexOf(openingTag) >= 0;
                        bool containsClosingTag = word.IndexOf(closingTag) >= 0;

                        bool containsOtherOpeningTags = IsStringContains(word, otherOpeningTags);
                        bool containsOtherClosingTags = IsStringContains(word, otherClosingTags);

                        if (!containsClosingTag && !containsOpeningTag && !containsOtherOpeningTags && !containsOtherClosingTags)
                        {
                            string tmpWord = words[x];
                            tmpWord = tmpWord.Insert(indexOfTrigger + trigger.Length, closingTag);
                            tmpWord = tmpWord.Insert(indexOfTrigger, openingTag);

                            words[x] = tmpWord;

                            break;
                        }
                    }
                }
            }
        }

        string tmp = "";
        for (int i = 0; i < words.Length; i++)
            tmp += " " + words[i];

        result = tmp;

        result = result.Replace(openingTag, CreateOpenSpan(cssClass));
        result = result.Replace(closingTag, "</span>");

        return result;
    }



    private bool IsStringContains(string inputSearchIn, List<List<string>> inputListOfListsOfStrings)
    {
        bool result = false;

        for (int x = 0; x < inputListOfListsOfStrings.Count; x++)
        {
            for (int y = 0; y < inputListOfListsOfStrings[x].Count; y++)
            {
                string target = inputListOfListsOfStrings[x][y].ToLower();
                string searchIn = inputSearchIn.ToLower();

                if (searchIn.Contains(target))
                {
                    result = true;
                    break;
                }
            }

            if (result)
                break;
        }

        return result;
    }

    private string CreateOpenSpan(string className)
    {
        return $"<span class='{className}'>";
    }

    public void AppendConfig(HtmlTextFormaterConfig htmlTextFormaterConfig)
    {
        HtmlTextFormaterConfig.MargeWith(htmlTextFormaterConfig);
    }

    public HtmlTextFormater()
    {
        HtmlTextFormaterConfig = new HtmlTextFormaterConfig();
    }
    public HtmlTextFormater(HtmlTextFormaterConfig htmlTextFormaterConfig)
    {
        HtmlTextFormaterConfig = htmlTextFormaterConfig;
    }
}
