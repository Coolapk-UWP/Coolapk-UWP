using System;
using System.ComponentModel;

namespace CoolapkUWP.Data
{
    class AppData : INotifyPropertyChanged
    {
        //缩略图
        private Uri thumbnail;
        public Uri Thumbnail
        {
            get { return thumbnail; }
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
            get { return title; }
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
            get { return describe; }
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
            get { return tag; }
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
