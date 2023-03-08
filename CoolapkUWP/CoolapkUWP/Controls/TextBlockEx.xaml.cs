using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Converters;
using CoolapkUWP.Models.Images;
using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.UI.Converters;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// 基于 Cyenoch 的 MyRichTextBlock 控件和 Coolapk UWP 项目的 TextBlockEx 控件重制
    /// </summary>
    public sealed partial class TextBlockEx : UserControl
    {
        public const string AuthorBorder = "<div class=\"author-border\"/>";
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Feed");

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TextBlockEx),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register(
                nameof(MaxLines),
                typeof(int),
                typeof(TextBlockEx),
                null);

        public static readonly DependencyProperty IsTextSelectionEnabledProperty =
            DependencyProperty.Register(
                nameof(IsTextSelectionEnabled),
                typeof(bool),
                typeof(TextBlockEx),
                new PropertyMetadata(false));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        public bool IsTextSelectionEnabled
        {
            get => (bool)GetValue(IsTextSelectionEnabledProperty);
            set => SetValue(IsTextSelectionEnabledProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextBlockEx).GetTextBlock();
        }

        public TextBlockEx() => InitializeComponent();

        private void GetTextBlock()
        {
            RichTextBlock.Blocks.Clear();
            HtmlDocument doc = new HtmlDocument();
            Regex emojis = new Regex(@"(\[\S*?\]|#\(\S*?\))");
            doc.LoadHtml(Text.Replace("<!--break-->", string.Empty));
            Paragraph paragraph = new Paragraph { LineHeight = FontSize + 10 };
            ImmutableArray<ImageModel>.Builder imageArrayBuider = ImmutableArray.CreateBuilder<ImageModel>();
            void NewLine()
            {
                RichTextBlock.Blocks.Add(paragraph);
                paragraph = new Paragraph { LineHeight = FontSize + 10 };
            }
            void AddText(string item) => paragraph.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(item) });
            HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
            foreach (HtmlNode node in nodes)
            {
                switch (node.NodeType)
                {
                    case HtmlNodeType.Text:
                        string[] list = emojis.Split(node.InnerText);
                        foreach (string item in list)
                        {
                            if (string.IsNullOrEmpty(item)) { continue; }
                            switch (item[0])
                            {
                                case '#':
                                    {
                                        string str = item.Substring(1);
                                        if (EmojiHelper.Emojis.Contains(str))
                                        {
                                            InlineUIContainer container = new InlineUIContainer();
                                            Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{str}.png")) };
                                            ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                            Viewbox viewbox = new Viewbox
                                            {
                                                Child = image,
                                                Margin = new Thickness(0, 0, 0, -4),
                                                VerticalAlignment = VerticalAlignment.Center
                                            };
                                            viewbox.SetBinding(WidthProperty, new Binding
                                            {
                                                Source = this,
                                                Mode = BindingMode.OneWay,
                                                Converter = new FontSizeToHeightConverter(),
                                                Path = new PropertyPath(nameof(FontSize))
                                            });
                                            container.Child = viewbox;
                                            paragraph.Inlines.Add(container);
                                        }
                                        else { AddText(item); }
                                    }
                                    break;
                                case '[':
                                    {
                                        if (SettingsHelper.Get<bool>("IsUseOldEmojiMode") && EmojiHelper.OldEmojis.Contains(item))
                                        {
                                            InlineUIContainer container = new InlineUIContainer();
                                            Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png")) };
                                            ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                            Viewbox viewbox = new Viewbox
                                            {
                                                Child = image,
                                                Margin = new Thickness(0, 0, 0, -4),
                                                VerticalAlignment = VerticalAlignment.Center
                                            };
                                            viewbox.SetBinding(WidthProperty, new Binding
                                            {
                                                Source = this,
                                                Mode = BindingMode.OneWay,
                                                Converter = new FontSizeToHeightConverter(),
                                                Path = new PropertyPath(nameof(FontSize))
                                            });
                                            container.Child = viewbox;
                                            paragraph.Inlines.Add(container);
                                        }
                                        else if (EmojiHelper.Emojis.Contains(item))
                                        {
                                            InlineUIContainer container = new InlineUIContainer();
                                            Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png")) };
                                            ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                            Viewbox viewbox = new Viewbox
                                            {
                                                Child = image,
                                                Margin = new Thickness(0, 0, 0, -4),
                                                VerticalAlignment = VerticalAlignment.Center
                                            };
                                            viewbox.SetBinding(WidthProperty, new Binding
                                            {
                                                Source = this,
                                                Mode = BindingMode.OneWay,
                                                Converter = new FontSizeToHeightConverter(),
                                                Path = new PropertyPath(nameof(FontSize))
                                            });
                                            container.Child = viewbox;
                                            paragraph.Inlines.Add(container);
                                        }
                                        else { AddText(item); }
                                        break;
                                    }
                                default:
                                    AddText(item);
                                    break;
                            }
                        }
                        break;
                    case HtmlNodeType.Element:
                        string content = node.InnerText;
                        switch (node.OriginalName)
                        {
                            case "a":
                                string tag = node.GetAttributeValue("t", string.Empty);
                                string href = node.GetAttributeValue("href", string.Empty);
                                string type = node.GetAttributeValue("type", string.Empty);
                                Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };
                                if (!string.IsNullOrEmpty(href))
                                {
                                    hyperlink.Click += (sender, e) => _ = UIHelper.OpenLinkAsync(href);
                                    ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                                }
                                if (!content.StartsWith("@") && !content.StartsWith("#") && !(type == "user-detail"))
                                {
                                    Run run2 = new Run { Text = "\uE167", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                                    hyperlink.Inlines.Add(run2);
                                }
                                else if (content == "查看图片" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
                                {
                                    content = "查看图片";
                                    Run run2 = new Run { Text = "\uE158", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                                    hyperlink.Inlines.Add(run2);
                                }
                                Run run3 = new Run { Text = WebUtility.HtmlDecode(content) };
                                hyperlink.Inlines.Add(run3);
                                paragraph.Inlines.Add(hyperlink);
                                break;

                            case "img":
                                string alt = node.GetAttributeValue("alt", string.Empty);
                                string src = node.GetAttributeValue("src", string.Empty);
                                int width = Convert.ToInt32(node.GetAttributeValue("width", "-1").Replace("\"", string.Empty));
                                int height = Convert.ToInt32(node.GetAttributeValue("height", "-1").Replace("\"", string.Empty));

                                ImageModel imageModel;
                                Image image = new Image();
                                InlineUIContainer container = new InlineUIContainer();

                                imageModel = new ImageModel(src, ImageType.OriginImage);
                                image.SetBinding(Image.SourceProperty, new Binding
                                {
                                    Source = imageModel,
                                    Mode = BindingMode.OneWay,
                                    Path = new PropertyPath(nameof(imageModel.Pic))
                                });
                                if (!string.IsNullOrEmpty(alt))
                                {
                                    ToolTipService.SetToolTip(image, new ToolTip { Content = alt });
                                }

                                if (src.Contains("emoticons"))
                                {
                                    Viewbox viewbox = new Viewbox
                                    {
                                        Child = image,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    viewbox.SetBinding(WidthProperty, new Binding
                                    {
                                        Source = this,
                                        Mode = BindingMode.OneWay,
                                        Converter = new FontSizeToHeightConverter(),
                                        Path = new PropertyPath(nameof(FontSize))
                                    });
                                    container.Child = viewbox;
                                    paragraph.Inlines.Add(container);
                                }
                                else
                                {
                                    NewLine();
                                    imageArrayBuider.Add(imageModel);

                                    Grid Grid = new Grid
                                    {
                                        CornerRadius = new CornerRadius(4)
                                    };

                                    StackPanel IsGIFPanel = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        HorizontalAlignment = HorizontalAlignment.Left
                                    };

                                    StackPanel PicSizePanel = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        HorizontalAlignment = HorizontalAlignment.Right
                                    };

                                    Border GIFBorder = new Border
                                    {
                                        CornerRadius = new CornerRadius(0, 0, 4, 0),
                                        Child = new TextBlock
                                        {
                                            Text = _loader.GetString("GIF"),
                                            Margin = new Thickness(2, 0, 2, 0)
                                        },
                                        Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88))
                                    };
                                    GIFBorder.SetBinding(VisibilityProperty, new Binding
                                    {
                                        Source = imageModel,
                                        Mode = BindingMode.OneWay,
                                        Converter = new BoolToVisibilityConverter(),
                                        Path = new PropertyPath(nameof(imageModel.IsGif))
                                    });

                                    IsGIFPanel.Children.Add(GIFBorder);

                                    Border WidePicBorder = new Border
                                    {
                                        CornerRadius = new CornerRadius(0, 0, 0, 4),
                                        Child = new TextBlock
                                        {
                                            Margin = new Thickness(2, 0, 2, 0),
                                            Text = _loader.GetString("WidePic.Text")
                                        },
                                        Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88))
                                    };
                                    WidePicBorder.SetBinding(VisibilityProperty, new Binding
                                    {
                                        Source = imageModel,
                                        Mode = BindingMode.OneWay,
                                        Converter = new BoolToVisibilityConverter(),
                                        Path = new PropertyPath(nameof(imageModel.IsWidePic))
                                    });

                                    Border LongPicTextBorder = new Border
                                    {
                                        CornerRadius = new CornerRadius(0, 0, 0, 4),
                                        Child = new TextBlock
                                        {
                                            Margin = new Thickness(2, 0, 2, 0),
                                            Text = _loader.GetString("LongPic.Text")
                                        },
                                        Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88))
                                    };
                                    LongPicTextBorder.SetBinding(VisibilityProperty, new Binding
                                    {
                                        Source = imageModel,
                                        Mode = BindingMode.OneWay,
                                        Converter = new BoolToVisibilityConverter(),
                                        Path = new PropertyPath(nameof(imageModel.IsLongPic))
                                    });

                                    PicSizePanel.Children.Add(WidePicBorder);
                                    PicSizePanel.Children.Add(LongPicTextBorder);

                                    Viewbox viewbox = new Viewbox
                                    {
                                        Child = image,
                                        Margin = new Thickness(0, 0, 0, -4),
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    if (width != -1) { viewbox.MaxWidth = width; }
                                    if (height != -1) { viewbox.MaxHeight = height; }

                                    Grid.Children.Add(viewbox);
                                    Grid.Children.Add(PicSizePanel);
                                    Grid.Tapped += (sender, args) => _ = UIHelper.ShowImageAsync(imageModel);

                                    container.Child = Grid;

                                    Paragraph paragraph1 = new Paragraph { TextAlignment = TextAlignment.Center };
                                    paragraph1.Inlines.Add(container);
                                    RichTextBlock.Blocks.Add(paragraph1);

                                    if (!string.IsNullOrEmpty(alt))
                                    {
                                        Paragraph paragraph2 = new Paragraph
                                        {
                                            LineHeight = FontSize + 10,
                                            TextAlignment = TextAlignment.Center,
                                        };
                                        Run run = new Run { Text = WebUtility.HtmlDecode(alt) };
                                        paragraph2.Inlines.Add(run);
                                        RichTextBlock.Blocks.Add(paragraph2);
                                    }
                                }
                                break;

                            case "div":
                                container = new InlineUIContainer();
                                Border border = new Border
                                {
                                    Margin = new Thickness(4, 0, 4, -4),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    BorderThickness = new Thickness(1),
                                    CornerRadius = new CornerRadius(4),
                                };
                                TextBlock textBlock = new TextBlock
                                {
                                    Margin = new Thickness(1),
                                    FontSize = 12,
                                    Text = _loader.GetString("FeedAuthor.Text"),
                                };

                                border.Child = textBlock;
                                container.Child = border;
                                paragraph.Inlines.Add(container);
                                break;

                            default: break;
                        }
                        break;
                }
            }

            ImmutableArray<ImageModel> array = imageArrayBuider.ToImmutable();
            foreach (ImageModel item in array)
            {
                item.ContextArray = array;
            }

            RichTextBlock.Blocks.Add(paragraph);
        }
    }
}
