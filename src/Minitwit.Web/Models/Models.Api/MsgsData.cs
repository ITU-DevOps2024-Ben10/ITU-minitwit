namespace Minitwit.Web.Models.Models.Api;

public class MsgsData : IData
{
    public string content { get; set; }

    public string GetData()
    {
        return ToString();
    }

    public override string ToString()
    {
        return $"{{content: {content}}}";
    }
}
