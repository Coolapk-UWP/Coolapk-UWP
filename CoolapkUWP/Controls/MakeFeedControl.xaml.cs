using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Controls.MakeFeedControlModel;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace CoolapkUWP.Controls
{
    public enum MakeFeedMode
    {
        Feed,
        Reply,
        ReplyReply
    }

    public sealed partial class MakeFeedControl : UserControl
    {
        public event EventHandler MakedFeedSuccessful;

        public MakeFeedMode MakeFeedMode { get; set; }

        public string FeedId { get; set; }

        public MakeFeedControl()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            inputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
            contentText = contentText.Replace("\r", "\r\n");
            if (string.IsNullOrWhiteSpace(contentText)) return;
            using (var content = new HttpMultipartFormDataContent(GetBoundary()))
            {
                switch (MakeFeedMode)
                {
                    case MakeFeedMode.Feed:
                        using (var a = new HttpStringContent(contentText))
                        using (var b = new HttpStringContent("feed"))
                        using (var c = new HttpStringContent("0"))
                        {
                            content.Add(a, "message");
                            content.Add(b, "type");
                            content.Add(c, "is_html_article");
                            await MakeFeed(UriType.CreateFeed, content);
                        }
                        break;

                    case MakeFeedMode.Reply:
                    case MakeFeedMode.ReplyReply:
                        var type = MakeFeedMode == MakeFeedMode.Reply ? UriType.CreateFeedReply : UriType.CreateReplyReply;
                        using (var d = new HttpStringContent(contentText))
                        {
                            content.Add(d, "message");
                            await MakeFeed(type, content);
                        }
                        break;
                }
            }
        }

        private async Task MakeFeed(UriType type, HttpMultipartFormDataContent content)
        {
            try
            {
                if (type == UriType.CreateFeed)
                {
                    if (await DataHelper.PostDataAsync(UriProvider.GetObject(type).GetUri(), content) != null)
                    {
                        SendSuccessful();
                    }
                }
                else
                {
                    if (await DataHelper.PostDataAsync(UriProvider.GetObject(type).GetUri(FeedId), content) != null)
                    {
                        SendSuccessful();
                    }
                }
            }
            catch (CoolapkMessageException cex)
            {
                UIHelper.ShowMessage(cex.Message);
                if (cex.MessageStatus == CoolapkMessageException.RequestCaptcha)
                {
                    var dialog = new CaptchaDialog();
                    await dialog.ShowAsync();
                }
            }
        }

        private void SendSuccessful()
        {
            UIHelper.ShowMessage(Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("MakeFeedControl").GetString("sendSuccessed"));
            inputBox.Document.SetText(TextSetOptions.None, string.Empty);
            MakedFeedSuccessful?.Invoke(this, new EventArgs());
        }

        private string GetBoundary()
        {
            byte[] vs = new byte[16];
            new Random().NextBytes(vs);
            var builder = new System.Text.StringBuilder();
            foreach (var item in vs)
                builder.Append(Convert.ToString(item, 16));
            builder.Insert(8, "-");
            builder.Insert(13, "-");
            builder.Insert(18, "-");
            builder.Insert(23, "-");
            return builder.ToString();
        }

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            await InsertEmoji(e.ClickedItem as EmojiData);
        }

        private async Task InsertEmoji(EmojiData data)
        {
            inputBox.Document.Selection.InsertImage(20,
                                                    20,
                                                    0,
                                                    VerticalCharacterAlignment.Baseline,
                                                    data.Name,
                                                    await (await StorageFile.GetFileFromApplicationUriAsync(data.Uri)).OpenReadAsync());
            inputBox.Document.Selection.MoveRight(TextRangeUnit.Character, 1, false);
        }

        private void InputBox_ContextMenuOpening(object sender, ContextMenuEventArgs e) => e.Handled = true;

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text) || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) { return; }

            sender.ItemsSource = (from s in EmojiTypeHelper.TypeHeaders
                                  from emoji in s.Emojis
                                  where emoji.Name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase)
                                  select emoji);
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = (args.SelectedItem as EmojiData).Name;
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion == null) { return; }
            await InsertEmoji(args.ChosenSuggestion as EmojiData);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (sender == userList)
            {
                inputBox.Document.Selection.TypeText($" @{((UserModel)e.ClickedItem).UserName} ");
            }
            else if (sender == topicList)
            {
                inputBox.Document.Selection.TypeText($" #{((TopicModel)e.ClickedItem).Title}# ");
            }
        }

        ViewModels.SearchPage.ViewModel viewModel = new ViewModels.SearchPage.ViewModel(1, string.Empty);

        ImmutableArray<AutoSuggestBox> boxes;
        ImmutableArray<Microsoft.UI.Xaml.Controls.ProgressRing> rings;

        private async void AutoSuggestBox_QuerySubmitted_1(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            for (int i = 0; i < boxes.Length; i++)
            {
                if (sender == boxes[i])
                {
                    rings[i].IsActive = true;
                    await viewModel.ChangeWordAndSearch(sender.Text, i + 1);
                    rings[i].IsActive = false;
                }
            }
        }

        private void Flyout_Opened(object sender, object e)
        {
            if (boxes == null)
            {
                boxes = new AutoSuggestBox[]
                {
                    autoSuggestBox0,
                    autoSuggestBox1,
                }.ToImmutableArray();
                rings = new Microsoft.UI.Xaml.Controls.ProgressRing[]
                {
                    ring0,
                    ring1,
                }.ToImmutableArray();
                userList.ItemsSource = viewModel.providers[1].Models;
                topicList.ItemsSource = viewModel.providers[2].Models;
            }
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var s = (ScrollViewer)sender;
            if (e.IsIntermediate && s.VerticalOffset == s.ScrollableHeight)
            {
                var i = searchPivot.SelectedIndex;
                rings[i].IsActive = true;
                await viewModel.ChangeWordAndSearch(boxes[i].Text, i + 1);
                rings[i].IsActive = false;
            }
        }
    }
}
