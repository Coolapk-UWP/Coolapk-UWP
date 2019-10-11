using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 酷安_UWP.Model;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DeveloperPage : Page
    {
        public DeveloperPage()
        {
            this.InitializeComponent();
            LoadDeveloper(LoginPage.source1, xl1);
            LoadDeveloper(LoginPage.source2, xl2);
        }


        private void LoadDeveloper(String str, ListView list)
        {
            //绑定一个列表
            ObservableCollection<AppData> Collection = new ObservableCollection<AppData>();
            list.ItemsSource = Collection;

            if (str.Equals("")) return;

            //循环添加AppData
            String body = Regex.Split(str, "<tbody>")[1];
            body = Regex.Split(body, "</tbody>")[0];
            String[] bodys = Regex.Split(body, @"\n");
            String[] bodylist = Regex.Split(body, @"<tr id=""data-row--");
            for (int i = 0; i < bodylist.Length - 1; i++)
            {
                AppData date = new AppData()
                {
                    Tag = bodys[i * 33 + 3 + 2].Split('"')[1],
                    Thumbnail = new Uri(bodys[i * 33 + 3 + 2].Split('"')[7], UriKind.RelativeOrAbsolute),
                    Title = bodys[i * 33 + 3 + 5].Split('>')[1].Split('<')[0],
                    Describe = bodys[i * 33 + 3 + 6].Split('>')[1].Split('<')[0]
                };
                Collection.Add(date);
            }
        }
    }
}
