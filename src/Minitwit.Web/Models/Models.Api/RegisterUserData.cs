namespace Minitwit.Web.Models.Models.Api;

public class RegisterUserData : IData
{
    public string username { get; set; }
    public string email { get; set; }
    public string pwd { get; set; }

    public string GetData()
    {
        return ToString();
    }

    public override string ToString()
    {
        return $"{{Username: {username}, Email: {email}, Password: {pwd}}}";
    }
}
