using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Exceptions;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnicodeStyle;
using UnicodeStyle.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using muxc = Microsoft.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class CreateFeedControl : Picker
    {
        private AppBarToggleButton BoldButton;
        private AppBarToggleButton ItalicButton;
        private AppBarToggleButton UnderLineButton;
        private AppBarToggleButton StrikethroughButton;

        public CreateFeedViewModel Provider;

        public static readonly DependencyProperty FeedTypeProperty =
            DependencyProperty.Register(
                nameof(FeedType),
                typeof(CreateFeedType),
                typeof(CreateFeedControl),
                new PropertyMetadata(CreateFeedType.Feed, OnFeedPropertyChanged));

        public CreateFeedType FeedType
        {
            get => (CreateFeedType)GetValue(FeedTypeProperty);
            set => SetValue(FeedTypeProperty, value);
        }

        public static readonly DependencyProperty ReplyIDProperty =
            DependencyProperty.Register(
                nameof(ReplyID),
                typeof(int),
                typeof(CreateFeedControl),
                new PropertyMetadata(0, OnFeedPropertyChanged));

        public int ReplyID
        {
            get => (int)GetValue(ReplyIDProperty);
            set => SetValue(ReplyIDProperty, value);
        }

        private static void OnFeedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CreateFeedControl)d).UpdateTitle();
        }

        public CreateFeedControl()
        {
            InitializeComponent();
            Provider = new CreateFeedViewModel();
            Clipboard.ContentChanged += Clipboard_ContentChanged;
            Clipboard_ContentChanged(null, null);
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            if (Provider != null)
            {
                switch (FeedType)
                {
                    case CreateFeedType.Feed:
                        Provider.Title = "写动态";
                        break;
                    case CreateFeedType.Reply:
                        Provider.Title = $"回复动态 ID {ReplyID}";
                        break;
                    case CreateFeedType.ReplyReply:
                        Provider.Title = $"回复评论 ID {ReplyID}";
                        break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "CloseButton":
                    Hide();
                    break;
                default:
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Send":
                    CreateDataContent();
                    break;
                case "AddPic":
                    Provider.PickImage();
                    break;
                default:
                    break;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "PastePic":
                    _ = Provider.DropFile(Clipboard.GetContent());
                    break;
                case "DeletePic":
                    Provider.Pictures.Remove((sender as FrameworkElement).Tag as WriteableBitmap);
                    break;
                default:
                    break;
            }
        }

        private void CreateDataContent()
        {
            UIHelper.ShowProgressBar();
            InputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
            contentText = contentText.Replace("\r", "\r\n");
            if (string.IsNullOrWhiteSpace(contentText)) { return; }
            if (FeedType == CreateFeedType.Feed) { CreateFeedContent(contentText); }
            else { CreateReplyContent(contentText); }
        }

        private async void CreateFeedContent(string contentText)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent message = new StringContent(contentText))
                using (StringContent type = new StringContent("feed"))
                using (StringContent is_html_article = new StringContent("0"))
                using (StringContent pic = new StringContent(string.Join(",", await Provider.UploadPic())))
                {
                    content.Add(message, "message");
                    content.Add(type, "type");
                    content.Add(is_html_article, "is_html_article");
                    content.Add(pic, "pic");
                    await SendContent(content);
                }
            }
        }

        private async void CreateReplyContent(string contentText)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent message = new StringContent(contentText))
                using (StringContent pic = new StringContent(string.Join(",", await Provider.UploadPic())))
                {
                    content.Add(message, "message");
                    content.Add(pic, "pic");
                    await SendContent(content);
                }
            }
        }

        private async Task SendContent(HttpContent content)
        {
            UriType type = (UriType)(-1);
            switch (FeedType)
            {
                case CreateFeedType.Feed:
                    type = UriType.CreateFeed;
                    break;
                case CreateFeedType.Reply:
                    type = UriType.CreateFeedReply;
                    break;
                case CreateFeedType.ReplyReply:
                    type = UriType.CreateReplyReply;
                    break;
            }

            try
            {
                object[] arg = Array.Empty<object>();
                if (type != UriType.CreateFeed)
                {
                    arg = new object[] { ReplyID };
                }
                (bool isSucceed, JToken _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, arg), content);
                if (isSucceed)
                {
                    SendSuccessful();
                }
            }
            catch (CoolapkMessageException cex)
            {
                UIHelper.ShowMessage(cex.Message);
                if (cex.MessageStatus == CoolapkMessageException.RequestCaptcha)
                {
                    //CaptchaDialog dialog = new CaptchaDialog();
                    //_ = await dialog.ShowAsync();
                }
            }
            UIHelper.HideProgressBar();
        }

        private void SendSuccessful()
        {
            UIHelper.ShowMessage(ResourceLoader.GetForViewIndependentUse("CreateFeedControl").GetString("SendSuccessed"));
            InputBox.Document.SetText(TextSetOptions.None, string.Empty);
            Provider.Pictures.Clear();
            Hide();
        }

        private void InputBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.RichEditBox", "ContextFlyout"))
            {
                InputBox.ContextFlyout.Opening += Menu_Opening;
                InputBox.ContextFlyout.Closing += Menu_Closing;
            }

            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.RichEditBox", "SelectionFlyout"))
            {
                InputBox.SelectionFlyout.Opening += Menu_Opening;
                InputBox.SelectionFlyout.Closing += Menu_Closing;
            }
        }

        private void Menu_Opening(object sender, object e)
        {
            if (sender is muxc.CommandBarFlyout Flyout && Flyout.Target == InputBox)
            {
                Flyout.PrimaryCommands.Clear();

                BoldButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE8DD",
                        FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"]
                    }
                };
                BoldButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(BoldButton);

                ItalicButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE8DB",
                        FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"]
                    }
                };
                ItalicButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(ItalicButton);

                Flyout.PrimaryCommands.Add(new AppBarSeparator());

                UnderLineButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE8DC",
                        FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"]
                    }
                };
                UnderLineButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(UnderLineButton);

                StrikethroughButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\u0335a\u0335b\u0335c\u0335",
                        FontFamily = (FontFamily)Application.Current.Resources["ContentControlThemeFontFamily"],
                        Margin = new Thickness(0, -5, 0, 0)
                    }
                };
                StrikethroughButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(StrikethroughButton);
            }
        }

        private void Menu_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
            if (BoldButton != null)
            {
                BoldButton.Click -= StyleButton_Click;
                BoldButton = null;
            }

            if (ItalicButton != null)
            {
                ItalicButton.Click -= StyleButton_Click;
                ItalicButton = null;
            }

            if (UnderLineButton != null)
            {
                UnderLineButton.Click -= StyleButton_Click;
                UnderLineButton = null;
            }

            if (StrikethroughButton != null)
            {
                StrikethroughButton.Click -= StyleButton_Click;
                StrikethroughButton = null;
            }
        }

        private void StyleButton_Click(object sender, RoutedEventArgs e)
        {
            if (BoldButton != null && ItalicButton != null && UnderLineButton != null && StrikethroughButton != null)
            {
                InputBox.Document.Selection.GetText(TextGetOptions.UseObjectText, out string SelectionText);

                SelectionText = UnderLineButton.IsChecked == true
                    ? UnicodeLiner.AddLine(SelectionText, true, UnicodeLines.Underline)
                    : UnicodeLiner.RemoveLine(SelectionText, UnicodeLines.Underline);

                SelectionText = StrikethroughButton.IsChecked == true
                    ? UnicodeLiner.AddLine(SelectionText, true, UnicodeLines.LongStrokeOverlay)
                    : UnicodeLiner.RemoveLine(SelectionText, UnicodeLines.LongStrokeOverlay);

                UnicodeStyles Style = BoldButton.IsChecked == true
                    ? ItalicButton.IsChecked == true
                        ? UnicodeStyles.BoldItalic
                        : UnicodeStyles.Bold
                    : ItalicButton.IsChecked == true
                        ? UnicodeStyles.Italic
                        : UnicodeStyles.Regular;

                using (UnicodeStyler Styler = new UnicodeStyler())
                {
                    SelectionText = Styler.StyleConvert(SelectionText, Style);
                }

                InputBox.Document.Selection.Text = SelectionText;
            }
        }

        private void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            InsertEmoji(e.ClickedItem.ToString());
            EmojiFlyout.Hide();
        }

        private async void InsertEmoji(string data)
        {
            string name = data[0] == '(' ? $"#{data}" : data;
            InputBox.Document.Selection.InsertImage(
                20, 20, 0,
                VerticalCharacterAlignment.Baseline,
                name,
                await (await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/Emoji/{data}.png"))).OpenReadAsync());
            _ = InputBox.Document.Selection.MoveRight(TextRangeUnit.Character, 1, false);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LinkFlyout.Hide();
            if (e.AddedItems.FirstOrDefault() is UserModel UserModel)
            {
                InputBox.Document.Selection.TypeText($"@{UserModel.UserName} ");
            }
            else if (e.AddedItems.FirstOrDefault() is TopicModel TopicModel)
            {
                InputBox.Document.Selection.TypeText($" #{TopicModel.Title}# ");
            }
            (sender as ListView).SelectedIndex = -1;
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.Handled = true;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            _ = Provider.DropFile(e.DataView);
            e.Handled = true;
        }

        private void Clipboard_ContentChanged(object sender, object e) => _ = Dispatcher.AwaitableRunAsync(async () => PastePic.IsEnabled = await Provider.CheckData(Clipboard.GetContent()));

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;

        #region 搜索框

        private void UserAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Provider.CreateUserItemSourse.Keyword = args.QueryText;
            _ = Provider.CreateUserItemSourse.Refresh(true);
        }

        private void TopicAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Provider.CreateTopicItemSourse.Keyword = args.QueryText;
            _ = Provider.CreateTopicItemSourse.Refresh(true);
        }

        private void EmojiAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && !string.IsNullOrWhiteSpace(sender.Text))
            {
                sender.ItemsSource = EmojiHelper.Emojis.Where(x => (x[0] == '(' ? $"#{x}" : x).Contains(sender.Text));
            }
        }

        private void EmojiAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            EmojiFlyout.Hide();
            InsertEmoji(args.SelectedItem.ToString());
            sender.Text = string.Empty;
        }

        #endregion
    }

    public class StringToEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object result = ImageCacheHelper.NoPic;
            string item = value.ToString();
            if (EmojiHelper.Emojis.Contains(item))
            {
                result = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png"));
            }
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return targetType.IsInstanceOfType(value) ? value : XamlBindingHelper.ConvertValue(targetType, value);
        }
    }

    public class EmojiNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string data = value.ToString();
            string result = data[0] == '(' ? $"#{data}" : data;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string data = value.ToString();
            string result = data[0] == '#' ? data.Substring(1) : data;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }
    }

    public enum CreateFeedType
    {
        Feed,
        Reply,
        ReplyReply
    }
}
