using CoolapkUWP.Helpers;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed partial class AboutControl : UserControl
    {
        public AboutControl()
        {
            InitializeComponent();
        }

        private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
            {
                string str = link.ToString();
                if (str.Contains("m.coolapk.com/mp")) { UIHelper.NavigateInSplitPane(typeof(HTMLTextPage), str); }
                else { UIHelper.Navigate(typeof(BrowserPage), new object[] { false, str }); }
            }
        }
    }
}