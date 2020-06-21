using CoolapkUWP.Helpers;
using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed partial class NotifyPopup : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool progressRingActived;

        public bool ProgressRingActived
        {
            get => progressRingActived;
            private set
            {
                progressRingActived = value;
                RaisePropertyChangedEvent();
            }
        }

        public NotifyPopup()
        {
            this.InitializeComponent();
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
            HorizontalAlignment = HorizontalAlignment.Center;

            UIHelper.InAppNotification = AppNotification;
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Height = e.Size.Height;
            Width = e.Size.Width;
        }
    }
}