using CoolapkUWP.Control.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Models.Pages
{
    internal abstract class FeedListDetailBase : Entity, INotifyPropertyChanged
    {
        private bool isCopyEnabled;

        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                isCopyEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListDetailBase(IJsonValue o) : base(o)
        {

        }
    }

    internal class UserDetail
    {
        public string UserFaceUrl;
        public ImageSource UserFace;
        public string UserName;
        public double FollowNum;
        public double FansNum;
        public double FeedNum;
        public double Level;
        public string Bio;
        public string BackgroundUrl;
        public string Verify_title;
        public string Gender;
        public string City;
        public string Astro;
        public string Logintime;
        public string FollowStatus;
        public ImageViewModel Background;
        public int SelectedIndex { get; set; }
        public bool ShowFollowStatus { get => !string.IsNullOrEmpty(FollowStatus); }
        public bool Has_bio { get => !string.IsNullOrEmpty(Bio); }
        public bool Has_verify_title { get => !string.IsNullOrEmpty(Verify_title); }
        public bool Has_Astro { get => !string.IsNullOrEmpty(Astro); }
        public bool Has_City { get => !string.IsNullOrWhiteSpace(City) && !string.IsNullOrEmpty(City); }
        public bool Has_Gender { get => !string.IsNullOrEmpty(Gender); }
    }

    internal class TopicDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public double FollowNum { get; set; }
        public double CommentNum { get; set; }
        public string Description { get; set; }
        public int SelectedIndex { get; set; }
    }

    internal class DYHDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double FollowNum { get; set; }
        public bool ShowUserButton { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public ImageSource UserAvatar { get; set; }
        public int SelectedIndex { get; set; }
        public bool ShowComboBox { get; set; }
    }

    internal class ProductDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public double FollowNum { get; set; }
        public string CommentNum { get; set; }
        public string Description { get; set; }
        public int SelectedIndex { get; set; }
    }

    internal class APPDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public double FollowNum { get; set; }
        public string CommentNum { get; set; }
        public string Description { get; set; }
        public int SelectedIndex { get; set; }
    }
}
