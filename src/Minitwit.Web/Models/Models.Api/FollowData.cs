namespace Minitwit.Web.Models.Models.Api;

public class FollowData : IData
{
    public string? follow { get; set; }
    public string? unfollow { get; set; }

    public string GetData() { return ToString(); }
    public override string ToString() { return $"{{follow: {follow}, unfollow: {unfollow}}}"; }

}