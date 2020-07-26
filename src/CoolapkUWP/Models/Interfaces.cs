namespace CoolapkUWP.Models
{
    internal interface ICanChangeLikModel
    {
        string Likenum { get; set; }
        bool Liked { get; set; }
        string Id { get; }
    }

    internal interface ICanChangeReplyNum
    {
        string Replynum { get; set; }
    }

    internal interface ICanCopy
    {
        bool IsCopyEnabled { get; set; }
    }
}