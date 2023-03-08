using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed partial class ShowQRCodeControl : UserControl
    {
        public static readonly DependencyProperty QRCodeTextProperty = DependencyProperty.Register(
            nameof(QRCodeText),
            typeof(string),
            typeof(ShowQRCodeControl),
            new PropertyMetadata("https://www.coolapk.com", new PropertyChangedCallback(OnQRCodeTextChanged))
        );

        public string QRCodeText
        {
            get => (string)GetValue(QRCodeTextProperty);
            set => SetValue(QRCodeTextProperty, value);
        }

        private static void OnQRCodeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ShowQRCodeControl).QRCodeText = e.NewValue as string ?? "https://www.coolapk.com"; ;
        }

        public ShowQRCodeControl() => InitializeComponent();

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ContentPresenter content = element.FindDescendant<ContentPresenter>();
            if (content != null)
            {
                content.CornerRadius = new CornerRadius(8);
            }
        }

        private void ShowUIButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();

            Uri shareLinkString = QRCodeText.ValidateAndGetUri();
            if (shareLinkString != null)
            {
                dataPackage.SetWebLink(shareLinkString);
                dataPackage.Properties.Title = "动态分享";
                dataPackage.Properties.Description = QRCodeText;
            }
            else
            {
                dataPackage.SetText(QRCodeText);
                dataPackage.Properties.Title = "内容分享";
                dataPackage.Properties.Description = "内含文本";
            }

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (obj, args) => { args.Request.Data = dataPackage; };
            DataTransferManager.ShowShareUI();
        }
    }
}