using System;
using System.ComponentModel;

namespace CoolapkUWP.Data
{
    internal class AppData : INotifyPropertyChanged
    {
        //缩略图
        private Uri thumbnail;
        public Uri Thumbnail
        {
            get => thumbnail;
            set
            {
                thumbnail = value;
                this.OnPropertyChanged("Thumbnail");
            }
        }

        //标题
        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                this.OnPropertyChanged("Title");
            }
        }

        //描述
        private string describe;
        public string Describe
        {
            get => describe;
            set
            {
                describe = value;
                this.OnPropertyChanged("Describe");
            }
        }

        //Tag
        private string tag;
        public string Tag
        {
            get => tag;
            set
            {
                tag = value;
                this.OnPropertyChanged("Tag");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
