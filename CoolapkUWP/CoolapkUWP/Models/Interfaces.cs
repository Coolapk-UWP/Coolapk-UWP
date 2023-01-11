using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models
{
    public interface IPic
    {
        string Uri { get; }
        ImageType Type { get; }
        BitmapImage Pic { get; }
    }

    public interface IHasTitle
    {
        string Url { get; }
        string Title { get; }
    }

    public interface IHasDescription : IHasTitle
    {
        ImageModel Pic { get; }
        string Description { get; }
    }

    public interface IHasSubtitle : IHasDescription
    {
        string SubTitle { get; }
    }

    public interface ICanCopy
    {
        bool IsCopyEnabled { get; set; }
    }

    public interface ICanFollowModel
    {
        int UID { get; }
        bool Followed { get; set; }
    }

    public interface ICanChangeReplyNum
    {
        int ID { get; }
        int ReplyNum { get; set; }
    }

    public interface ICanChangeLikeModel
    {
        int ID { get; }
        bool Liked { get; set; }
        int LikeNum { get; set; }
    }

    public interface ICanChangeStarModel
    {
        int ID { get; }
        bool Stared { get; set; }
        int StarNum { get; set; }
    }
}
