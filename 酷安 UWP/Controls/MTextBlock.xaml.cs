using CoolapkUWP.Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;



namespace CoolapkUWP.Controls
{
    /// <summary>
    /// 用于显示带表情和超链接的控件。使用MessageText指定要显示的内容。
    /// </summary>
    public sealed partial class MTextBlock : Page
    {
        private string _messageText;
        public string MessageText
        {
            get => _messageText;
            set
            {
                if (value.Replace("<!--break-->", string.Empty) != _messageText)
                {
                    _messageText = value.Replace("<!--break-->", string.Empty);
                    GetTextBlock();
                }
            }
        }

        Dictionary<string, string> uris = new Dictionary<string, string>();
        public MTextBlock() => this.InitializeComponent();
        async void GetTextBlock()
        {
            Regex hrefRegex = new Regex("href=\".+?\"");
            string xamlContent = "<Paragraph>";
            List<string> list = GetStringList();
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item))
                    xamlContent += "\n</Paragraph>\n<Paragraph>";
                else switch (item[0])
                    {
                        case '<':
                            string content = item.Substring(item.IndexOf('>') + 1, item.LastIndexOf('<') - item.IndexOf('>') - 1);
                            string href = string.Empty;
                            var match = hrefRegex.Match(item);
                            if (match.Success)
                                href = match.Value.Substring(match.Value.IndexOf('"') + 1, match.Value.LastIndexOf('"') - match.Value.IndexOf('"') - 1);
                            if (item.Contains("t=\"image\""))
                            {
                                bool isGif = href.Substring(href.LastIndexOf('.')).ToLower().Contains("gif");
                                xamlContent += $@"
</Paragraph>
<Paragraph TextAlignment='Center' Foreground='Gray'>
    <InlineUIContainer>
        <Grid>
            <Image Source='{await ImageCacheHelper.GetImagePath(ImageType.SmallImage, href)}' Tag='{href}' ToolTipService.ToolTip='{content}'/>";
                                if (isGif)
                                    xamlContent += @"
            <Border Background='{ThemeResource SystemControlBackgroundAccentBrush}' VerticalAlignment='Bottom' HorizontalAlignment='Right'>
                <TextBlock>GIF</TextBlock>
            </Border>
        </Grid>
    </InlineUIContainer>";
                                else xamlContent += @"
        </Grid>
    </InlineUIContainer>";
                                xamlContent += $@"
    <Run Text='{content}'/>
</Paragraph>
<Paragraph>";
                            }
                            else
                            {
                                string n = $"uri{uris.Count}";
                                uris.Add(n, href);
                                xamlContent += $@"
    <Hyperlink Click='Hyperlink_Click' ToolTipService.ToolTip='{GetStringInXML(href)}'>
        <Run Text='{GetStringInXML(content)}' Name='{n}'/>
    </Hyperlink>";
                            }
                            break;
                        case '#':
                            string s = item.Substring(1, item.Length - 2);
                            if (EmojiHelper.Contains(s))
                                xamlContent += $@"
    <InlineUIContainer>
        <Image Source='{$"/Assets/Emoji/{s}.png"}' Height='20' Width='20' ToolTipService.ToolTip='{item}'/>
    </InlineUIContainer>";
                            else xamlContent += $@"<Run Text='{item}'/>";
                            break;
                        case '[':
                            if (SettingsHelper.Get<bool>("IsUseOldEmojiMode") && EmojiHelper.Contains(item, true))
                                xamlContent += $@"
    <InlineUIContainer>
        <Image Source='{$"/Assets/Emoji/{item}2.png"}' Height='20' Width='20' ToolTipService.ToolTip='{item}'/>
    </InlineUIContainer>";
                            else if (EmojiHelper.Contains(item))
                                xamlContent += $@"
    <InlineUIContainer>
        <Image Source='{$"/Assets/Emoji/{item}.png"}' Height='20' Width='20' ToolTipService.ToolTip='{item}'/>
    </InlineUIContainer>";
                            else xamlContent += $@"
    <Run Text='{item}'/>";
                            break;
                        default:
                            xamlContent += $@"
    <Run Text='{GetStringInXML(item)}'/>";
                            break;
                    }
            }
            xamlContent += "\n</Paragraph>";
            try
            {
                var box = XamlReader.Load($@"
<RichTextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml\' TextWrapping='Wrap' IsTextSelectionEnabled='False' >
    {xamlContent}
</RichTextBlock>") as RichTextBlock;
                foreach (var item in box.Blocks)
                    if (item is Paragraph paragraph)
                        foreach (var i in paragraph.Inlines)
                            if (i is Hyperlink hyperlink)
                                hyperlink.Click += (sender, e) =>
                                {
                                    string s = (sender.Inlines[0] as Run).Name;
                                    if (s == "查看图片" && (uris[s].IndexOf("http://image.coolapk.com") == 0 || uris[s].IndexOf("https://image.coolapk.com") == 0))
                                        UIHelper.ShowImage(uris[s], ImageType.SmallImage);
                                    else
                                        UIHelper.OpenLink(uris[s]);
                                };
                            else if (i is InlineUIContainer container)
                                if (container.Child is Grid grid)
                                    if (grid.Children[0] is Image image && !string.IsNullOrEmpty(image.Tag as string))
                                        image.Tapped += (s, e) => UIHelper.ShowImage((s as FrameworkElement).Tag as string, ImageType.SmallImage);
                MainBorder.Child = box;
            }
            catch (System.Exception e)
            {
                throw new System.Exception($"文本内容显示失败。\n\n{e.Message}\n\nXAML：\n{xamlContent}\n\n", e);
            }
        }

        string GetStringInXML(string originString) => originString.Replace("&amp;", "&#38;").Replace("&#038;", "&#38;")
                                                                  .Replace("<", "&#60;").Replace("&lt;", "&#60;").Replace("&#060;", "&#60;")
                                                                  .Replace(">", "&#62;").Replace("&gt;", "&#62;").Replace("&#062;", "&#62;")
                                                                  .Replace("\"", "&#34;").Replace("&quot;", "&#34;").Replace("&#034;", "&#34;")
                                                                  .Replace("'", "&#39;").Replace("&apos;", "&#39;").Replace("&#039;", "&#39;")
                                                                  .Replace(" ", "&#160;").Replace("&nbsp;", "&#160;");
        //偷懒没有写 & => &#38; 的转换，应该不会出问题吧

        List<string> GetStringList()
        {
            Regex linkRegex = new Regex("<a[^>]*?>.*?</a>"), emojiRegex1 = new Regex(@"\[\S*?\]"), emojiRegex2 = new Regex(@"#\(\S*?\)");
            List<string> result = new List<string>();

            //处理超链接或图文中的图片
            for (int i = 0; i < MessageText.Length;)
            {
                var matchedValue = linkRegex.Match(MessageText, i);
                int index = (string.IsNullOrEmpty(matchedValue.Value) ? MessageText.Length : MessageText.IndexOf(matchedValue.Value, i)) - i;
                if (index == 0)
                {
                    result.Add(matchedValue.Value);
                    i += matchedValue.Length;
                }
                else if (index > 0)
                {
                    result.Add(MessageText.Substring(i, index));
                    i += index;
                }
            }

            //处理[..]样式的表情
            for (int j = 0; j < result.Count; j++)
            {
                for (int i = 0; i < result[j].Length;)
                {
                    var v = emojiRegex1.Match(result[j], i);
                    int a = string.IsNullOrEmpty(v.Value) ? -1 : result[j].IndexOf(v.Value, i) - i;
                    if (a == 0)
                    {
                        if (EmojiHelper.Contains(result[j].Substring(0, v.Length)) && (emojiRegex1.IsMatch(result[j], i + v.Length) || result[j].Length > v.Length))
                        {
                            result.Insert(j + 1, result[j].Substring(v.Length));
                            result[j] = result[j].Substring(0, v.Length);
                        }
                        i += v.Length;
                    }
                    else if (a > 0)
                    {
                        if (EmojiHelper.Contains(result[j].Substring(a, v.Length)))
                        {
                            result.Insert(j + 1, result[j].Substring(a));
                            result[j] = result[j].Substring(0, a);
                        }
                        i += a;
                    }
                    else if (a == -1) break;
                }
            }

            //处理#(..)样式的表情
            for (int j = 0; j < result.Count; j++)
            {
                for (int i = 0; i < result[j].Length;)
                {
                    var v = emojiRegex2.Match(result[j], i);
                    int a = string.IsNullOrEmpty(v.Value) ? -1 : result[j].IndexOf(v.Value, i) - i;
                    if (a == 0)
                    {
                        if (EmojiHelper.Contains(result[j].Substring(1, v.Length - 2)) && emojiRegex2.IsMatch(result[j], i + v.Length) || result[j].Length > v.Length)
                        {
                            result.Insert(j + 1, result[j].Substring(v.Length));
                            result[j] = result[j].Substring(0, v.Length);
                        }
                        i += v.Length;
                    }
                    else if (a > 0)
                    {
                        if (EmojiHelper.Contains(result[j].Substring(a + 1, v.Length - 2)))
                        {
                            result.Insert(j + 1, result[j].Substring(a));
                            result[j] = result[j].Substring(0, a);
                        }
                        i += a;
                    }
                    else if (a == -1) break;
                }
            }

            //处理 \n
            for (int j = 0; j < result.Count; j++)
            {
                for (int i = 0; i < result[j].Length;)
                {
                    int a = result[j].IndexOf("\n", i) == -1 ? -1 : result[j].IndexOf("\n", i) - i;
                    if (a == 0)
                    {
                        if (!linkRegex.IsMatch(result[j]) && result[j].Length > 1)
                        {
                            result.Insert(j + 1, result[j].Substring(1));
                            result[j] = string.Empty;
                        }
                        i += 1;
                    }
                    else if (a > 0)
                    {
                        if (!linkRegex.IsMatch(result[j]))
                        {
                            result.Insert(j + 1, result[j].Substring(a));
                            result[j] = result[j].Substring(0, a);
                        }
                        i += a;
                    }
                    else if (a == -1) break;
                }
            }

            return result;
        }
    }
}
