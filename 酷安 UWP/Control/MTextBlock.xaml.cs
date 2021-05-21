using CoolapkUWP.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Control
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
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
            foreach (var item in GetStringList())
            {
                if (string.IsNullOrEmpty(item))
                    xamlContent += "</Paragraph><Paragraph>";
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
            <Image Source='{await ImageCache.GetImagePath(ImageType.SmallImage, href)}' Tag='{href}' ToolTipService.ToolTip='{content}'/>";
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
                                if (!uris.ContainsKey(content)) uris.Add(content, href);
                                xamlContent += $@"
<Hyperlink Click='Hyperlink_Click' ToolTipService.ToolTip='{href}'>
    <Run Text='{content}'/>
</Hyperlink>";
                            }
                            break;
                        case '#':
                            string s = item.Substring(1, item.Length - 2);
                            if (Emojis.emojis.Contains(s))
                                xamlContent += $@"
<InlineUIContainer>
    <Image Source='{$"/Assets/Emoji/{s}.png"}' Height='20' Width='20' Margin='-1, 0, 0, -4' ToolTipService.ToolTip='{item}'/>
</InlineUIContainer>";
                            else xamlContent += $@"<Run Text='{item}'/>";
                            break;
                        case '[':
                            if (Settings.GetBoolen("IsUseOldEmojiMode") && Emojis.oldEmojis.Contains(item))
                                xamlContent += $@"
<InlineUIContainer>
    <Image Source='{$"/Assets/Emoji/{item}2.png"}' Height='20' Width='20' Margin='-1, 0, 0, -4' ToolTipService.ToolTip='{item}'/>
</InlineUIContainer>";
                            else if (Emojis.emojis.Contains(item))
                                xamlContent += $@"
<InlineUIContainer>
    <Image Source='{$"/Assets/Emoji/{item}.png"}' Height='20' Width='20' Margin='-1, 0, 0, -4' ToolTipService.ToolTip='{item}'/>
</InlineUIContainer>";
                            else xamlContent += $@"<Run Text='{item}'/>";
                            break;
                        default:
                            xamlContent += $@"<Run Text='{item}'/>";
                            break;
                    }
            }
            xamlContent += "</Paragraph>";
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
                                string s = (sender.Inlines[0] as Run).Text;
                                if (s == "查看图片" && uris[s].IndexOf("http://image.coolapk.com") == 0)
                                    Tools.ShowImage(uris[s], ImageType.SmallImage);
                                else
                                    Tools.OpenLink(uris[s]);
                            };
                        else if (i is InlineUIContainer container)
                            if (container.Child is Grid grid)
                                if (grid.Children[0] is Image image && !string.IsNullOrEmpty(image.Tag as string))
                                    image.Tapped += (s, e) => Tools.ShowImage((s as FrameworkElement).Tag as string, ImageType.SmallImage);
            MainBorder.Child = box;
        }

        List<string> GetStringList()
        {
            Regex linkRegex = new Regex("<a.*?>.*?</a>"), emojiRegex = new Regex(@"\[\S*?\]|#\(\S*?\)");
            List<string> result = new List<string>();
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
            for (int j = 0; j < result.Count; j++)
            {
                for (int i = 0; i < result[j].Length;)
                {
                    var v = emojiRegex.Match(result[j], i);
                    int a = string.IsNullOrEmpty(v.Value) ? -1 : result[j].IndexOf(v.Value, i) - i;
                    if (a == 0)
                    {
                        if (emojiRegex.IsMatch(result[j], i + v.Length) || result[j].Length > v.Length)
                        {
                            result.Insert(j + 1, result[j].Substring(v.Length));
                            result[j] = result[j].Substring(0, v.Length);
                        }
                        i += v.Length;
                    }
                    else if (a > 0)
                    {
                        result.Insert(j + 1, result[j].Substring(a));
                        result[j] = result[j].Substring(0, a);
                        i += a;
                    }
                    else if (a == -1) break;
                }
            }
            for (int j = 0; j < result.Count; j++)
            {
                for (int i = 0; i < result[j].Length;)
                {
                    int a = result[j].IndexOf("\n", i) == -1 ? -1 : result[j].IndexOf("\n", i) - i;
                    if (a == 0)
                    {
                        if (result[j].IndexOf("\n", i + 1) != -1 || result[j].Length > 1)
                            result.Insert(j + 1, result[j].Substring(1));
                        result[j] = string.Empty;
                        i += 1;
                    }
                    else if (a > 0)
                    {
                        result.Insert(j + 1, result[j].Substring(a));
                        result[j] = result[j].Substring(0, a);
                        i += a;
                    }
                    else if (a == -1) break;
                }
            }
            return result;
        }
    }
}
