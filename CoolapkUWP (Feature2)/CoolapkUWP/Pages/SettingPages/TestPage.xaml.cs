using CoolapkUWP.Control;
using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Contacts;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            InitializeComponent();
            tile.Text = ApplicationData.Current.LocalSettings.Values["TileUrl"].ToString();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            { Frame.GoBack(); }
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Uri uri = ValidateAndGetUri(url.Text);
            string s = uri == null ? "这不是一个链接" : await UIHelper.GetHTML(uri.ToString(), "XMLHttpRequest");
            if (string.IsNullOrEmpty(s))
            {
                s = "网络错误";
            }
            ContentDialog GetJsonDialog = new ContentDialog
            {
                Title = url.Text,
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = ConvertJsonString(s),
                        IsTextSelectionEnabled = true
                    },
                    VerticalScrollMode = ScrollMode.Enabled,
                    HorizontalScrollMode = ScrollMode.Enabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                },
                CloseButtonText = "好的",
                DefaultButton = ContentDialogButton.Close
            };
            _ = await GetJsonDialog.ShowAsync();
        }

        private static Uri ValidateAndGetUri(string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString);
            }
            catch (FormatException)
            {
            }
            return uri;
        }

        private string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = null;
            try { obj = serializer.Deserialize(jtr); } catch { }
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["TileUrl"] = tile.Text;
            LiveTileControl.UpdateTile();
        }

        // Gets the rectangle of the element 
        public static Rect GetElementRect(FrameworkElement element)
        {
            // Passing "null" means set to root element. 
            GeneralTransform elementTransform = element.TransformToVisual(null);
            Rect rect = elementTransform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            return rect;
        }

        // Display a contact in response to an event
        private async void OnUserClickShowContactCard(object sender, RoutedEventArgs e)
        {
            string str = await UIHelper.GetJson("/user/profile?uid=" + await UIHelper.GetUserIDByName(contact.Text));
            if (!string.IsNullOrEmpty(str))
            {
                JsonObject json = UIHelper.GetJSonObject(str);
                if (json != null)
                {
                    UserViewModel UserModel = new UserViewModel(json);
                    if (ContactManager.IsShowContactCardSupported())
                    {
                        Rect selectionRect = GetElementRect((FrameworkElement)sender);

                        // Retrieve the contact to display
                        Contact contact = new Contact();
                        if (!string.IsNullOrEmpty(UserModel.Email))
                        {
                            ContactEmail email = new ContactEmail
                            {
                                Kind = ContactEmailKind.Personal,
                                Address = UserModel.Email
                            };
                            contact.Emails.Add(email);
                        }
                        if (!string.IsNullOrEmpty(UserModel.City))
                        {
                            ContactAddress address = new ContactAddress
                            {
                                Locality = UserModel.City
                            };
                            contact.Addresses.Add(address);
                        }
                        if (!string.IsNullOrEmpty(UserModel.Mobile))
                        {
                            ContactPhone phone = new ContactPhone
                            {
                                Number = UserModel.Mobile,
                                Kind = ContactPhoneKind.Mobile
                            };
                            contact.Phones.Add(phone);
                        }
                        if (!string.IsNullOrEmpty(UserModel.Url))
                        {
                            ContactWebsite website = new ContactWebsite
                            {
                                Uri = new Uri("https://www.coolapk.com" + UserModel.Url)
                            };
                            contact.Websites.Add(website);
                        }
                        ContactDate date = new ContactDate();
                        if (!string.IsNullOrEmpty(UserModel.BirthYear))
                        {
                            date.Year = int.Parse(UserModel.BirthYear);
                        }
                        if (!string.IsNullOrEmpty(UserModel.BirthMonth))
                        {
                            date.Month = uint.Parse(UserModel.BirthMonth);
                        }
                        if (!string.IsNullOrEmpty(UserModel.BirthDay))
                        {
                            date.Day = uint.Parse(UserModel.BirthDay);
                        }
                        if (date != null)
                        {
                            date.Kind = ContactDateKind.Birthday;
                            contact.ImportantDates.Add(date);
                        }
                        if (!string.IsNullOrEmpty(UserModel.UserName))
                        {
                            contact.Name = UserModel.UserName;
                        }
                        if (!string.IsNullOrEmpty(UserModel.Bio))
                        {
                            contact.Notes = UserModel.Bio;
                        }

                        ContactManager.ShowContactCard(contact, selectionRect, Placement.Default);
                    }
                }
            }
        }

        private void EmojisTest()
        {
            List<string> b;
            string emojis = string.Empty;
            b = EmojiHelper.normal.ToList();
            foreach (string item in EmojiHelper.coolcoins)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.funny)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.doge)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.tradition)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.classic)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.emojis)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in b)
            {
                emojis += $"\"{item}\",\n";
            }
            ContentDialog GetJsonDialog = new ContentDialog
            {
                Title = $"{b.ToArray().Length}/{EmojiHelper.emojis.Length}",
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = emojis,
                        IsTextSelectionEnabled = true
                    }
                },
                CloseButtonText = "好的",
                DefaultButton = ContentDialogButton.Close
            };
            _ = GetJsonDialog.ShowAsync();
        }

        private void uid_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click(sender, e);
            }
        }

        private void url_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_5(sender, e);
            }
        }

        private void message_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_4(sender, e);
            }
        }

        private void image_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_6(sender, e);
            }
        }

        private void tile_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_2(sender, e);
            }
        }

        private void contact_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                OnUserClickShowContactCard(sender, e);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(FeedPages.FeedListPage), new object[] { FeedPages.FeedListType.UserPageList, await UIHelper.GetUserIDByName(uid.Text) });

        private void Button_Click_1(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(AppPages.AppRecommendPage));

        private void Button_Click_4(object sender, RoutedEventArgs e) => UIHelper.ShowMessage(message.Text);

        private void Button_Click_6(object sender, RoutedEventArgs e) => UIHelper.ShowImage(image.Text, ImageType.OriginImage);
    }
}