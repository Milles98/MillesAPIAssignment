namespace AdvertisementAPI.Models
{
    /// <summary>
    /// Model for login
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Result of login
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
    }

}
