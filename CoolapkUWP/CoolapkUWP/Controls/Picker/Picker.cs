using CoolapkUWP.Helpers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkUWP.Controls
{
    public class Picker : ContentControl
    {
        private Popup _popup;
        private Grid _rootGrid;

        public static readonly DependencyProperty PopupTransitionsProperty =
            DependencyProperty.Register(
                nameof(PopupTransitions),
                typeof(TransitionCollection),
                typeof(Picker),
                null);

        public TransitionCollection PopupTransitions
        {
            get => (TransitionCollection)GetValue(PopupTransitionsProperty);
            set => SetValue(PopupTransitionsProperty, value);
        }

        public Picker()
        {
            DefaultStyleKey = typeof(Picker);
            Unloaded += Picker_Unloaded;
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (_rootGrid != null)
            {
                _rootGrid.Width = Window.Current.Bounds.Width;
                _rootGrid.Height = Window.Current.Bounds.Height;
            }
        }

        private void Picker_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= Picker_Unloaded;
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        public void Show(UIElement element)
        {
            if (Parent is Grid grid)
            {
                grid.Children.Remove(this);
            }

            _rootGrid = new Grid();

            _popup = new Popup
            {
                Child = _rootGrid
            };
            _rootGrid.Children.Add(this);

            _popup.SetXAMLRoot(element);

            _popup.IsOpen = true;
        }

        public void Hide()
        {
            _rootGrid?.Children.Remove(this);
            _rootGrid = null;

            if (_popup != null)
            {
                _popup.IsOpen = false;
            }
            _popup = null;
        }
    }
}
