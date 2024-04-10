namespace AdvertisementAPI.Models
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResult
    {
        public string Token { get; set; }
    }

}
