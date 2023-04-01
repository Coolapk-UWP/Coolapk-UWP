using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Threading.Tasks;
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

    public interface ICanLike
    {
        int ID { get; }
        bool Liked { get; set; }
        Task ChangeLike();
    }

    public interface ICanStar
    {
        int ID { get; }
        bool Stared { get; set; }
        int StarNum { get; set; }
    }

    public interface ICanReply
    {
        int ID { get; }
        int ReplyNum { get; set; }
    }

    public interface ICanFollow
    {
        int ID { get; }
        bool Followed { get; set; }
        Task ChangeFollow();
    }

    public interface IUserModel
    {
        int FansNum { get; }
        int FollowNum { get; }

        string Bio { get; }
        string Url { get; }
        string UserName { get; }
        string LoginTime { get; }

        ImageModel Cover { get; }
        ImageModel UserAvatar { get; }
    }
}
