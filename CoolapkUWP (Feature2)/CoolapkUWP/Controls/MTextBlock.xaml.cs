using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Control
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MTextBlock : Page
    {
        public const string AuthorBorder = "<div class=\"author-border\"></div>";
        public event EventHandler RichTextBlockLoaded;
        private RichTextBlock mainContent;
        private string _messageText;

        public string MessageText
        {
            get => _messageText;
            set
            {
                string str = value.Replace("<!--break-->", string.Empty);
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
                if (value >= 0)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (mainContent != null)
                        {
                            mainContent.MaxLines = value;
                            mainContent.TextTrimming = value > 0 ? TextTrimming.WordEllipsis : TextTrimming.None;
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

        private readonly Dictionary<string, string> uris = new Dictionary<string, string>();
        public MTextBlock() => InitializeComponent();

        private async void GetTextBlock()
        {
            RichTextBlock block = new RichTextBlock
            {
                IsTextSelectionEnabled = IsTextSelectionEnabled,
                TextWrapping = TextWrapping.Wrap,
            };
            Paragraph paragraph = new Paragraph();

            void NewLine()
            {
                block.Blocks.Add(paragraph);
                paragraph = new Paragraph();
            }
            void AddText(string item) => paragraph.Inlines.Add(new Run { Text = item.Replace("&amp;", "&").Replace("&#039;", "'").Replace("&quot;", "\"") });

            ImmutableArray<ImageData>.Builder imageArrayBuider = ImmutableArray.CreateBuilder<ImageData>();
            ImmutableArray<string> list = await GetStringList(_messageText);
            foreach (string item in list)
            {
                if (string.IsNullOrEmpty(item)) { NewLine(); }
                else
                {
                    switch (item[0])
                    {
                        case '<':
                            {
                                string content = string.Empty;
                                Regex[] contentRegex = new Regex[] { new Regex(@">(.*?)<"), new Regex(@"alt\s*=\s*""(.*?)""") };
                                if (contentRegex[0].IsMatch(item) && !string.IsNullOrEmpty(contentRegex[0].Match(item).Groups[1].Value))
                                {
                                    content = contentRegex[0].Match(item).Groups[1].Value;
                                }
                                else if (contentRegex[1].IsMatch(item) && !string.IsNullOrEmpty(contentRegex[1].Match(item).Groups[1].Value))
                                {
                                    content = contentRegex[1].Match(item).Groups[1].Value;
                                }
                                string href = string.Empty;
                                Regex[] hrefRegex = new Regex[] { new Regex(@"href\s*=\s*""(.*?)"""), new Regex(@"src\s*=\s*""(.*?)""") };
                                if (hrefRegex[0].IsMatch(item) && !string.IsNullOrEmpty(hrefRegex[0].Match(item).Groups[1].Value))
                                {
                                    href = hrefRegex[0].Match(item).Groups[1].Value;
                                }
                                else if (hrefRegex[1].IsMatch(item) && !string.IsNullOrEmpty(hrefRegex[1].Match(item).Groups[1].Value))
                                {
                                    href = hrefRegex[1].Match(item).Groups[1].Value;
                                }

                                if (item.Contains("t=\"image\""))
                                {
                                    NewLine();

                                    ImageData imageModel = new ImageData
                                    {
                                        Pic = await ImageCache.GetImage(ImageType.SmallImage, href),
                                        url = href
                                    };
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
                                        UIHelper.ShowImage(imageModel.url, ImageType.OriginImage);
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
                                            Child = new TextBlock { Text = "GIF" },
                                            Background = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0))
                                        };

                                        Border border2 = new Border
                                        {
                                            Child = new TextBlock { Text = "长图" },
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
                                            Child = new TextBlock { Text = "长图" },
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
                                    Run run = new Run { Text = content.Replace("&amp;", "&").Replace("&#039;", "'").Replace("<br/>", "\n") };
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
                                        Text = "楼主",
                                    };

                                    border.Child = textBlock;
                                    container.Child = border;
                                    paragraph.Inlines.Add(container);
                                }
                                else if (href.Contains("emoticons") && (href.EndsWith(".png") || href.EndsWith(".jpg") || href.EndsWith(".jpeg") || href.EndsWith(".gif") || href.EndsWith(".bmp") || href.EndsWith(".PNG") || href.EndsWith(".JPG") || href.EndsWith(".JPEG") || href.EndsWith(".GIF") || href.EndsWith(".BMP")))
                                {
                                    InlineUIContainer container = new InlineUIContainer();
                                    ImageData imageModel = new ImageData
                                    {
                                        Pic = await ImageCache.GetImage(ImageType.OriginImage, href),
                                        url = href
                                    };

                                    Image image = new Image
                                    {
                                        Height = Width = 20,
                                        Margin = new Thickness(0, 0, 0, -4),
                                    };
                                    image.SetBinding(Image.SourceProperty, new Binding
                                    {
                                        Source = imageModel,
                                        Path = new PropertyPath(nameof(imageModel.Pic)),
                                        Mode = BindingMode.OneWay
                                    });
                                    ToolTipService.SetToolTip(image, new ToolTip { Content = content });
                                    container.Child = image;
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
                                        content = "查看图片";
                                        Run run2 = new Run { Text = "\uE158", FontFamily = new FontFamily("Segoe MDL2 Assets") };
                                        hyperlink.Inlines.Add(run2);
                                    }
                                    Run run = new Run { Text = content.Replace("&amp;", "&").Replace("&#039;", "'") };
                                    hyperlink.Inlines.Add(run);
                                    hyperlink.Click += (sender, e) =>
                                    {
                                        if (content == "查看图片" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
                                        {
                                            UIHelper.ShowImage(href, ImageType.SmallImage);
                                        }
                                        else if (href.EndsWith(".png") || href.EndsWith(".jpg") || href.EndsWith(".jpeg") || href.EndsWith(".gif") || href.EndsWith(".bmp") || href.EndsWith(".PNG") || href.EndsWith(".JPG") || href.EndsWith(".JPEG") || href.EndsWith(".GIF") || href.EndsWith(".BMP"))
                                        {
                                            UIHelper.ShowImage(href, ImageType.OriginImage);
                                        }
                                        else
                                        {
                                            UIHelper.OpenLink(href);
                                        }
                                    };

                                    paragraph.Inlines.Add(hyperlink);
                                }
                            }
                            break;

                        case '#':
                            {
                                string s = item.Substring(1, item.Length - 2);
                                if (EmojiHelper.emojis.Contains(s))
                                {
                                    InlineUIContainer container = new InlineUIContainer();
                                    Image image = new Image
                                    {
                                        Height = Width = 20,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{s}.png"))
                                    };
                                    ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                    container.Child = image;
                                    paragraph.Inlines.Add(container);
                                }
                                else { AddText(item); }
                            }
                            break;
                        case '[':
                            {
                                if (SettingsHelper.GetBoolen("IsUseOldEmojiMode") && EmojiHelper.oldEmojis.Contains(item))
                                {
                                    InlineUIContainer container = new InlineUIContainer();
                                    Image image = new Image
                                    {
                                        Height = Width = 20,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}2.png"))
                                    };
                                    ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                    container.Child = image;
                                    paragraph.Inlines.Add(container);
                                }
                                else if (EmojiHelper.emojis.Contains(item))
                                {
                                    InlineUIContainer container = new InlineUIContainer();
                                    Image image = new Image
                                    {
                                        Height = Width = 20,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png"))
                                    };
                                    ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                    container.Child = image;
                                    paragraph.Inlines.Add(container);
                                }
                                else { AddText(item); }
                                break;
                            }

                        default: AddText(item); break;
                    }
                }
            }

            ImmutableArray<ImageData> array = imageArrayBuider.ToImmutable();
            foreach (ImageData item in array)
            {
                item.ContextArray = array;
            }

            block.Blocks.Add(paragraph);
            block.Height = block.Width = Height = Width = double.NaN;
            block.MaxHeight = block.MaxWidth = MaxHeight = MaxWidth = double.PositiveInfinity;

            Content = mainContent = block;

            if (MaxLine > 0)
            {
                block.MaxLines = MaxLine;
                block.TextTrimming = TextTrimming.WordEllipsis;
            }
            RichTextBlockLoaded?.Invoke(this, null);
        }

        private static Task<ImmutableArray<string>> GetStringList(string text)
        {
            return Task.Run(() =>
            {
                Regex link = new Regex(@"<\w+.*?>?.*?<?.*?/[\w|\s]*?>");
                Regex emojis = new Regex(@"\[\S*?\]|#\(\S*?\)");
                ImmutableArray<string>.Builder buider = ImmutableArray.CreateBuilder<string>();

                //处理超链接或图文中的图片
                for (int i = 0; i < text.Length;)
                {
                    Match matchedValue = link.Match(text, i);
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
                int length = AuthorBorder.Length;
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
                for (int j = 0; j < buider.Count; j++)
                {
                    for (int i = 0; i < buider[j].Length;)
                    {
                        var v = emojis.Match(buider[j], i);
                        int a = string.IsNullOrEmpty(v.Value) ? -1 : buider[j].IndexOf(v.Value, i) - i;
                        if (a == 0)
                        {
                            if (emojis.IsMatch(buider[j], i + v.Length) || buider[j].Length > v.Length)
                            {
                                buider.Insert(j + 1, buider[j].Substring(v.Length));
                                buider[j] = buider[j].Substring(0, v.Length);
                            }
                            i += v.Length;
                        }
                        else if (a > 0)
                        {
                            buider.Insert(j + 1, buider[j].Substring(a));
                            buider[j] = buider[j].Substring(0, a);
                            i += a;
                        }
                        else if (a == -1) break;
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
