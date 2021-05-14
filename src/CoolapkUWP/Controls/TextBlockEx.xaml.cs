using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.ValueConverters;
using CoolapkUWP.Models;
using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Controls
{
    /// <summary> 用于显示带表情和超链接的控件。使用MessageText指定要显示的内容。 </summary>
    public sealed partial class TextBlockEx : UserControl
    {
        public const string AuthorBorder = "<div class=\"author-border\"></div>";

        RichTextBlock mainContent;

        private string _messageText;

        public string MessageText
        {
            get => _messageText;
            set
            {
                var str = value.Replace("<!--break-->", string.Empty, StringComparison.OrdinalIgnoreCase);
                if (str != _messageText)
                {
                    _messageText = str;
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        GetTextBlock();
                    });
                }
            }
        }

        public int MaxLine
        {
            get => mainContent?.MaxLines ?? 0;
            set
            {
                //UIHelper.StatusBar_ShowMessage(value.ToString());
                if (value >= 0)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (mainContent != null)
                        {
                            mainContent.MaxLines = value;
                            mainContent.TextTrimming = value > 0 ? TextTrimming.WordEllipsis : TextTrimming.None;
                            //UIHelper.StatusBar_ShowMessage(mainContent.MaxLines.ToString());
                        }
                    });
                }
            }
        }

        public bool IsTextSelectionEnabled
        {
            get => mainContent?.IsTextSelectionEnabled ?? false;
            set => _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (mainContent != null)
                {
                    mainContent.IsTextSelectionEnabled = value;
                }
            });
        }

        public TextBlockEx() => InitializeComponent();

        public event EventHandler RichTextBlockLoaded;

        private async void GetTextBlock()
        {
            var block = new RichTextBlock
            {
                IsTextSelectionEnabled = IsTextSelectionEnabled,
                TextWrapping = TextWrapping.Wrap,
            };
            var paragraph = new Paragraph();

            void NewLine()
            {
                block.Blocks.Add(paragraph);
                paragraph = new Paragraph();
            }
            void AddText(string item) => paragraph.Inlines.Add(new Run { Text = item.Replace("&amp;", "&").Replace("&quot;", "\"") });

            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Feed");
            var imageArrayBuider = ImmutableArray.CreateBuilder<ImageModel>();
            var list = await GetStringList(_messageText);
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item)) { NewLine(); }
                else
                {
                    switch (item[0])
                    {
                        case '<':
                            {
                                string content = item.Substring(item.IndexOf('>') + 1, item.LastIndexOf('<') - item.IndexOf('>') - 1);
                                string href = string.Empty;
                                var hrefRegex = new Regex("href=\"(\\S|\\s)+?\"");
                                if (hrefRegex.IsMatch(item))
                                {
                                    var match = hrefRegex.Match(item);
                                    href = match.Value.Substring(match.Value.IndexOf('"') + 1, match.Value.LastIndexOf('"') - match.Value.IndexOf('"') - 1);
                                }

                                if (item.Contains("t=\"image\"", StringComparison.Ordinal))
                                {
                                    NewLine();

                                    var imageModel = new ImageModel(href, ImageType.SmallImage);
                                    imageArrayBuider.Add(imageModel);

                                    InlineUIContainer container = new InlineUIContainer();

                                    Image image = new Image
                                    {
                                        MaxHeight = MaxWidth = 400,
                                        MinHeight = MinWidth = 56,
                                        Stretch = Stretch.Uniform
                                    };
                                    image.SetBinding(Image.SourceProperty, new Binding
                                    {
                                        Source = imageModel,
                                        Path = new PropertyPath(nameof(imageModel.Pic)),
                                        Mode = BindingMode.OneWay
                                    });

                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        ToolTipService.SetToolTip(image, new ToolTip { Content = content });
                                    }
                                    image.Tapped += (sender, e) =>
                                    {
                                        e.Handled = true;
                                        UIHelper.ShowImage(imageModel);
                                    };

                                    Grid grid = new Grid();
                                    if (imageModel.IsGif)
                                    {
                                        StackPanel panel = new StackPanel
                                        {
                                            Orientation = Orientation.Horizontal,
                                            VerticalAlignment = VerticalAlignment.Top,
                                            HorizontalAlignment = HorizontalAlignment.Right,
                                            Margin = new Thickness(4)
                                        };
                                        Border border1 = new Border
                                        {
                                            Child = new TextBlock { Text = loader.GetString("GIF") },
                                            Background = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0))
                                        };

                                        Border border2 = new Border
                                        {
                                            Child = new TextBlock { Text = loader.GetString("longPicText") },
                                            Background = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0))
                                        };
                                        border2.SetBinding(VisibilityProperty, new Binding
                                        {
                                            Source = imageModel,
                                            Path = new PropertyPath(nameof(imageModel.IsLongPic)),
                                            Mode = BindingMode.OneWay,
                                            Converter = new BoolToVisibilityConverter()
                                        });

                                        panel.Children.Add(border1);
                                        grid.Children.Add(image);
                                        grid.Children.Add(panel);
                                    }
                                    else
                                    {
                                        Border border = new Border
                                        {
                                            Child = new TextBlock { Text = loader.GetString("longPicText") },
                                            Background = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0))
                                        };
                                        border.SetBinding(VisibilityProperty, new Binding
                                        {
                                            Source = imageModel,
                                            Path = new PropertyPath(nameof(imageModel.IsLongPic)),
                                            Mode = BindingMode.OneWay,
                                            Converter = new BoolToVisibilityConverter()
                                        });

                                        grid.Children.Add(image);
                                        grid.Children.Add(border);
                                    }
                                    container.Child = grid;
                                    Paragraph paragraph1 = new Paragraph { TextAlignment = TextAlignment.Center };
                                    paragraph1.Inlines.Add(container);
                                    block.Blocks.Add(paragraph1);

                                    Paragraph paragraph2 = new Paragraph
                                    {
                                        TextAlignment = TextAlignment.Center,
                                        Foreground = Windows.UI.Xaml.Application.Current.Resources["GrayText"] as SolidColorBrush
                                    };
                                    Run run = new Run { Text = content.Replace("&amp;", "&").Replace("<br/>", "\n") };
                                    paragraph2.Inlines.Add(run);
                                    block.Blocks.Add(paragraph2);
                                }
                                else if (item == AuthorBorder)
                                {
                                    InlineUIContainer container = new InlineUIContainer();
                                    Border border = new Border
                                    {
                                        Margin = new Thickness(4, 0, 4, -4),
                                        VerticalAlignment = VerticalAlignment.Center,
                                        BorderBrush = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["GrayText"],
                                        BorderThickness = new Thickness(1),
                                        CornerRadius = new CornerRadius(4),
                                    };
                                    TextBlock textBlock = new TextBlock
                                    {
                                        Margin = new Thickness(1),
                                        FontSize = 12,
                                        Text = loader.GetString("feedAuthorText"),
                                    };

                                    border.Child = textBlock;
                                    container.Child = border;
                                    paragraph.Inlines.Add(container);
                                }
                                else
                                {
                                    Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };
                                    if (!string.IsNullOrEmpty(href))
                                    {
                                        ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                                    }

                                    if (content.IndexOf('@') != 0 && content.IndexOf('#') != 0 && !item.Contains("type=\"user-detail\""))
                                    {
                                        Run run2 = new Run { Text = "\uE167", FontFamily = new FontFamily("Segoe MDL2 Assets") };
                                        hyperlink.Inlines.Add(run2);
                                    }
                                    else if (content == "查看图片" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
                                    {
                                        content = loader.GetString("seePic");
                                        Run run2 = new Run { Text = "\uE158", FontFamily = new FontFamily("Segoe MDL2 Assets") };
                                        hyperlink.Inlines.Add(run2);
                                    }
                                    Run run = new Run { Text = content.Replace("&amp;", "&") };
                                    hyperlink.Inlines.Add(run);
                                    hyperlink.Click += (sender, e) =>
                                    {
                                        if (content == loader.GetString("seePic") && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
                                        {
                                            UIHelper.ShowImage(href, ImageType.SmallImage);
                                        }
                                        else
                                        {
                                            UIHelper.OpenLinkAsync(href);
                                        }
                                    };

                                    paragraph.Inlines.Add(hyperlink);
                                }
                            }
                            break;

                        case '#':
                        case '[':
                            {
                                if (EmojiHelper.Contains(item))
                                {
                                    InlineUIContainer container = new InlineUIContainer();

                                    var useOld = item[0] != '#' && SettingsHelper.Get<bool>(SettingsHelper.IsUseOldEmojiMode) && EmojiHelper.Contains(item, true);
                                    Image image = new Image
                                    {
                                        Height = Width = 20,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        Source = new BitmapImage(EmojiHelper.Get(item, useOld))
                                    };
                                    ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                    container.Child = image;
                                    paragraph.Inlines.Add(container);

                                }
                                else { AddText(item); }
                            }
                            break;

                        default: AddText(item); break;
                    }
                }
            }

            var array = imageArrayBuider.ToImmutable();
            foreach (var item in array)
            {
                item.ContextArray = array;
            }

            block.Blocks.Add(paragraph);
            block.Height = block.Width = Height = Width = double.NaN;
            block.MaxHeight = block.MaxWidth = MaxHeight = MaxWidth = double.PositiveInfinity;

            Content = mainContent = block;

            await Task.Delay(20);
            if (MaxLine > 0)
            {
                block.MaxLines = MaxLine;
                block.TextTrimming = TextTrimming.WordEllipsis;
            }
            RichTextBlockLoaded?.Invoke(this, null);
            //UIHelper.StatusBar_ShowMessage(MaxLine.ToString());
        }

        private static Task<ImmutableArray<string>> GetStringList(string text)
        {
            return Task.Run(() =>
            {
                var link = new Regex("<a(\\S|\\s)*?>(\\S|\\s)*?</a>");
                var emojis = new Regex[] { new Regex(@"\[\S*?\]"), new Regex(@"#\(\S*?\)") };
                var buider = ImmutableArray.CreateBuilder<string>();

                //处理超链接或图文中的图片
                for (int i = 0; i < text.Length;)
                {
                    var matchedValue = link.Match(text, i);
                    int index = (string.IsNullOrEmpty(matchedValue.Value) ? text.Length : text.IndexOf(matchedValue.Value, i, StringComparison.Ordinal)) - i;
                    if (index == 0)
                    {
                        buider.Add(matchedValue.Value.Replace("\n", "<br/>"));
                        i += matchedValue.Length;
                    }
                    else if (index > 0)
                    {
                        buider.Add(text.Substring(i, index));
                        i += index;
                    }
                }
                //(IsFeedAuthor ? $"[{loader.GetString("feedAuthorText")}
                var length = AuthorBorder.Length;
                for (int j = 0; j < buider.Count; j++)
                {
                    for (int i = 0; i < buider[j].Length;)
                    {
                        int a = buider[j].IndexOf(AuthorBorder, i, StringComparison.Ordinal) == -1 ? -1 : buider[j].IndexOf(AuthorBorder, i, StringComparison.Ordinal) - i;
                        if (a == 0)
                        {
                            if (buider[j].Length > length)
                            {
                                buider.Insert(j + 1, buider[j].Substring(length));
                                buider[j] = AuthorBorder;
                            }
                            i += length;
                        }
                        else if (a > 0)
                        {
                            buider.Insert(j + 1, buider[j].Substring(a));
                            buider[j] = buider[j].Substring(0, a);
                            i += a;
                        }
                        else if (a == -1) { break; }
                    }
                }

                //处理表情
                for (int k = 0; k < emojis.Length; k++)
                {
                    for (int j = 0; j < buider.Count; j++)
                    {
                        for (int i = 0; i < buider[j].Length;)
                        {
                            var v = emojis[k].Match(buider[j], i);
                            int a = string.IsNullOrEmpty(v.Value) ? -1 : buider[j].IndexOf(v.Value, i, StringComparison.Ordinal) - i;
                            if (a == 0)
                            {
                                if (EmojiHelper.Contains(buider[j].Substring(0, v.Length)) && (emojis[k].IsMatch(buider[j], i + v.Length) || buider[j].Length > v.Length))
                                {
                                    buider.Insert(j + 1, buider[j].Substring(v.Length));
                                    buider[j] = buider[j].Substring(0, v.Length);
                                }
                                i += v.Length;
                            }
                            else if (a > 0)
                            {
                                if (EmojiHelper.Contains(buider[j].Substring(a, v.Length)))
                                {
                                    buider.Insert(j + 1, buider[j].Substring(a));
                                    buider[j] = buider[j].Substring(0, a);
                                }
                                i += a;
                            }
                            else if (a == -1) { break; }
                        }
                    }
                }

                ////处理 \n
                for (int j = 0; j < buider.Count; j++)
                {
                    for (int i = 0; i < buider[j].Length;)
                    {
                        int a = buider[j].IndexOf("\n", i, StringComparison.Ordinal) == -1 ? -1 : buider[j].IndexOf("\n", i, StringComparison.Ordinal) - i;
                        if (a == 0)
                        {
                            if (!link.IsMatch(buider[j]) && buider[j].Length > 1)
                            {
                                buider.Insert(j + 1, buider[j].Substring(1));
                                buider[j] = string.Empty;
                            }
                            i += 1;
                        }
                        else if (a > 0)
                        {
                            if (!link.IsMatch(buider[j]))
                            {
                                buider.Insert(j + 1, buider[j].Substring(a));
                                buider[j] = buider[j].Substring(0, a);
                            }
                            i += a;
                        }
                        else if (a == -1) { break; }
                    }
                }

                return buider.ToImmutableArray();
            });
        }
    }
}