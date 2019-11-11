using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Control
{
    public sealed partial class TitleBar : UserControl
    {
        public string Title { get => title.Text; set => title.Text = value; }
        public event RoutedEventHandler BackButtonClick;
        public event RoutedEventHandler RefreshButtonClick;
        public Visibility BackButtonVisibility { get => BackButton.Visibility; set => BackButton.Visibility = value; }
        public Visibility RefreshButtonVisibility { get => RefreshButton.Visibility; set => RefreshButton.Visibility = value; }
        public double TitleHeight { get => titleGrid.Height; set => titleGrid.Height = value; }
        public Symbol BackButtonSymbol { get => BackButtonIcon.Symbol; set => BackButtonIcon.Symbol= value; }
        public object RefreshButtonTag { get => RefreshButton.Tag; set => RefreshButton.Tag = value; }
        public TitleBar()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => BackButtonClick?.Invoke(sender, e);
        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshButtonClick?.Invoke(sender, e);
    }
}
