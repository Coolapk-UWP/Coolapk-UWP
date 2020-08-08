using CoolapkUWP.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models.Controls.MakeFeedControlModel
{
    internal class EmojiTypeHeader
    {
        public string Name { get; private set; }
        public ImmutableArray<EmojiData> Emojis { get; private set; }

        public EmojiTypeHeader(string name, ImmutableArray<EmojiData> emojis)
        {
            Name = name;
            Emojis = emojis;
        }
    }

    internal class EmojiData : INotifyPropertyChanged
    {
        private WeakReference<ImageSource> emoji;

        public ImageSource Emoji
        {
            get
            {
                if (emoji != null && emoji.TryGetTarget(out ImageSource source))
                {
                    return source;
                }
                else
                {
                    GetImage();
                    return null;
                }
            }
            private set
            {
                if (emoji == null)
                {
                    emoji = new WeakReference<ImageSource>(value);
                }
                else
                {
                    emoji.SetTarget(value);
                }
                RaisePropertyChangedEvent();
            }
        }

        public Uri Uri { get; private set; }
        public string Name { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public EmojiData(string name) => Name = name;

        private async void GetImage()
        {
            var uri = EmojiHelper.Get(Name);
            Uri = uri;
            await Task.Delay(20);
            Emoji = new BitmapImage(uri);
        }
    }
}
