using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace 酷安_UWP
{
    public sealed partial class SearchPage : Page
    {
        public string searchLink = "";
        MainPage mainPage;
        public SearchPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            searchLink = (string)((object[])e.Parameter)[0];
            mainPage = ((object[])e.Parameter)[1] as MainPage;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (searchLink.Equals("")) return;
            await ToSearch(searchLink);
        }

        private async Task ToSearch(string str)
        {
            try
            {
                SearchList.Items.Clear();
                SearchLoad(await Web.GetHttp("https://www.coolapk.com/search?q=" + str));
            }
            catch (Exception)
            {
            }
        }

        private void SearchLoad(String str)
        {
            mainPage.ActiveProgressRing();String body = Regex.Split(str, @"<div class=""left_nav"">")[1];
            body = Regex.Split(body, @"<div class=""panel-footer ex-card-footer text-center"">")[0];
            //&nbsp;处理
            body = body.Replace("&nbsp;", " ");
            String[] bodylist = Regex.Split(body, @"<a href=""");
            String[] bodys = Regex.Split(body, @"\n");
            for (int i = 0; i < bodylist.Length - 1; i++)
            {
                Grid newGrid = new Grid
                {
                    Height = 80,
                    Tag = bodys[i * 15 + 5].Split('"')[1]
                };

                Image newImage = new Image();
                try
                {
                    newImage = new Image
                    {
                        Margin = new Thickness(20, 20, 0, 20),
                        Width = 40,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Source = new BitmapImage(new Uri(bodys[i * 15 + 5 + 3].Split('"')[3], UriKind.RelativeOrAbsolute))
                    };
                }
                catch (Exception)
                {
                }

                TextBlock newText1 = new TextBlock
                {
                    Margin = new Thickness(80, 10, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = new SolidColorBrush(Colors.Black),
                    TextWrapping = TextWrapping.Wrap,
                    Text = bodys[i * 15 + 5 + 5].Split('>')[1].Split('<')[0]
                };
                TextBlock newText2 = new TextBlock
                {
                    Margin = new Thickness(80, 30, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                    TextWrapping = TextWrapping.Wrap,
                    Text = bodys[i * 15 + 5 + 6].Split('>')[1].Split('<')[0]
                };
                TextBlock newText3 = new TextBlock
                {
                    Margin = new Thickness(80, 50, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                    TextWrapping = TextWrapping.Wrap,
                    Text = bodys[i * 15 + 5 + 7].Split('>')[1].Split('<')[0]
                };
                newGrid.Children.Add(newImage);
                newGrid.Children.Add(newText1);
                newGrid.Children.Add(newText2);
                newGrid.Children.Add(newText3);
                SearchList.Items.Add(newGrid);
                mainPage.DeactiveProgressRing();
            }

        }

        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchList.SelectedIndex == -1) return;

            Frame.Navigate(typeof(AppPage), new object[] { "https://www.coolapk.com" + ((Grid)SearchList.Items[SearchList.SelectedIndex]).Tag.ToString(), mainPage });
            SearchList.SelectedIndex = -1;
        }


        private void SearchText_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SearchButton_Click(null, null);
        }


        private async void SearchButton_Click(object sender, RoutedEventArgs e) => await ToSearch(SearchText.Text);

        private void SearchBack_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
    }

}


/*
            < Grid Height = "80" >


                     < Image HorizontalAlignment = "Left" Margin = "20,20,0,20" Width = "40" />


                          < TextBlock HorizontalAlignment = "Left" Margin = "80,10,0,0" TextWrapping = "Wrap" Text = "应用名" VerticalAlignment = "Top" />


                                   < TextBlock HorizontalAlignment = "Left" Margin = "80,30,0,0" TextWrapping = "Wrap" Text = "分数 大小" VerticalAlignment = "Top" Foreground = "#99000000" />


                                              < TextBlock HorizontalAlignment = "Left" Margin = "80,50,0,0" TextWrapping = "Wrap" Text = "下载量" VerticalAlignment = "Top" Foreground = "#99000000" />


                                                     </ Grid >
                                                     */
