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

    public interface IList
    {
        string Url { get; }
        string Title { get; }
        ImageModel Pic { get; }
        string Description { get; }
    }

    public interface IHasUriAndTitle
    {
        string Url { get; }
        string Title { get; }
    }

    public interface IListWithSubtitle
    {
        string Url { get; }
        string Title { get; }
        ImageModel Pic { get; }
        string SubTitle { get; }
        string Description { get; }
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
