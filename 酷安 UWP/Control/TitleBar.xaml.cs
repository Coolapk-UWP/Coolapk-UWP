using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Control
{
    public sealed partial class TitleBar : UserControl
    {
        public string Title { get => title.Text; set => title.Text = value; }
        public event RoutedEventHandler RefreshEvent;
        public event RoutedEventHandler BackButtonClick;
        public event SelectionChangedEventHandler ComboBoxSelectionChange;
        public Symbol BackButtonSymbol { get => BackButtonIcon.Symbol; set => BackButtonIcon.Symbol = value; }
        public double TitleHeight { get => titleGrid.Height; set => titleGrid.Height = value; }
        public Visibility ComboBoxVisibility { get => ChangeModeComboBox.Visibility; set => ChangeModeComboBox.Visibility = value; }
        public object ComboBoxItemsSource { set => ChangeModeComboBox.ItemsSource = value; }
        public int ComboBoxSelectedIndex { get => ChangeModeComboBox.SelectedIndex; set => ChangeModeComboBox.SelectedIndex = value; }
        public object RightAreaContent { get => userContentPresenter.Content; set => userContentPresenter.Content = value; }

        public Visibility BackButtonVisibility { get => BackButton.Visibility; set => BackButton.Visibility = value; }
        public Visibility RefreshButtonVisibility { get => RefreshButton.Visibility; set => RefreshButton.Visibility = value; }
        public Visibility BackgroundVisibility { get => TitleBackground.Visibility; set => TitleBackground.Visibility = value; }

        public TitleBar() => this.InitializeComponent();
        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshEvent?.Invoke(sender, e);
        private void BackButton_Click(object sender, RoutedEventArgs e) => BackButtonClick?.Invoke(sender, e);
        private void ChangeModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ComboBoxSelectionChange?.Invoke(sender, e);
        private void titleGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Grid || (e.OriginalSource is TextBlock a && a == title))
            { RefreshEvent?.Invoke(sender, e); }
        }
    }
}
