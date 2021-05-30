﻿using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Controls.MakeFeedControlModel;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            inputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
            contentText = contentText.Replace("\r", "\r\n");
            if (string.IsNullOrWhiteSpace(contentText)) return;
            using (MultipartFormDataContent content = new MultipartFormDataContent(GetBoundary()))
            {
                switch (MakeFeedMode)
                {
                    case MakeFeedMode.Feed:
                        using (StringContent a = new StringContent(contentText))
                        using (StringContent b = new StringContent("feed"))
                        using (StringContent c = new StringContent("0"))
                        {
                            content.Add(a, "message");
                            content.Add(b, "type");
                            content.Add(c, "is_html_article");
                            await MakeFeed(UriType.CreateFeed, content);
                        }
                        break;

                    case MakeFeedMode.Reply:
                    case MakeFeedMode.ReplyReply:
                        UriType type = MakeFeedMode == MakeFeedMode.Reply ? UriType.CreateFeedReply : UriType.CreateReplyReply;
                        using (StringContent d = new StringContent(contentText))
                        {
                            content.Add(d, "message");
                            if (!string.IsNullOrEmpty(Uri.Text)) { content.Add(new StringContent(Uri.Text), "pic"); }
                            await MakeFeed(type, content);
                        }
                        break;
                }
            }
        }

        private async Task MakeFeed(UriType type, HttpContent content)
        {
            try
            {
                object[] arg = Array.Empty<object>();
                if (type != UriType.CreateFeed)
                {
                    arg = new object[] { FeedId };
                }
                (bool r, Newtonsoft.Json.Linq.JToken _) = await DataHelper.PostDataAsync(UriHelper.GetUri(type, arg), content);
                if (r)
                {
                    SendSuccessful();
                }
            }
            catch (Core.Exceptions.CoolapkMessageException cex)
            {
                UIHelper.ShowMessage(cex.Message);
                if (cex.MessageStatus == Core.Exceptions.CoolapkMessageException.RequestCaptcha)
                {
                    CaptchaDialog dialog = new CaptchaDialog();
                    _ = await dialog.ShowAsync();
                }
            }
        }

        private void SendSuccessful()
        {
            UIHelper.ShowMessage(Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("MakeFeedControl").GetString("sendSuccessed"));
            inputBox.Document.SetText(TextSetOptions.None, string.Empty);
            MakedFeedSuccessful?.Invoke(this, new EventArgs());
        }

        private static string GetBoundary()
        {
            byte[] vs = new byte[16];
            new Random().NextBytes(vs);
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (byte item in vs) { _ = builder.Append(Convert.ToString(item, 16)); }
            _ = builder.Insert(8, "-");
            _ = builder.Insert(13, "-");
            _ = builder.Insert(18, "-");
            _ = builder.Insert(23, "-");
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
            _ = inputBox.Document.Selection.MoveRight(TextRangeUnit.Character, 1, false);
        }

        internal static void InputBox_ContextMenuOpening(object sender, ContextMenuEventArgs e) => e.Handled = true;

        internal static void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text) || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) { return; }

            sender.ItemsSource = (from s in EmojiTypeHelper.TypeHeaders
                                  from emoji in s.Emojis
                                  where emoji.Name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase)
                                  select emoji);
        }

        internal static void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = (args.SelectedItem as EmojiData).Name;
        }

        internal async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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
            if (boxes.IsDefaultOrEmpty)
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
            ScrollViewer s = (ScrollViewer)sender;
            if (e.IsIntermediate && s.VerticalOffset == s.ScrollableHeight)
            {
                int i = searchPivot.SelectedIndex;
                rings[i].IsActive = true;
                await viewModel.ChangeWordAndSearch(boxes[i].Text, i + 1);
                rings[i].IsActive = false;
            }
        }
    }
}
