using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed partial class TitleBar : UserControl
    {
        public bool IsBackButtonEnabled { get => BackButton.IsEnabled; set => BackButton.IsEnabled = value; }

        public string Title { get => title.Text; set => title.Text = value; }

        public event RoutedEventHandler RefreshButtonClicked;

        public event RoutedEventHandler BackButtonClicked;

        public Visibility BackButtonVisibility { get => BackButton.Visibility; set => BackButton.Visibility = value; }
        public Visibility RefreshButtonVisibility { get => RefreshButton.Visibility; set => RefreshButton.Visibility = value; }

        public double TitleHeight { get => titleGrid.Height; set => titleGrid.Height = value; }
        public object RightAreaContent { get => userContentPresenter.Content; set => userContentPresenter.Content = value; }

        public TitleBar() => this.InitializeComponent();

        private void BackButton_Click(object sender, RoutedEventArgs e) => BackButtonClicked?.Invoke(sender, e);

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshButtonClicked?.Invoke(sender, e);

        public void ShowProgressRing()
        {
            progressRing.Visibility = Visibility.Visible;
            progressRing.IsActive = true;
        }

        public void HideProgressRing()
        {
            progressRing.Visibility = Visibility.Collapsed;
            progressRing.IsActive = false;
        }
    }
}