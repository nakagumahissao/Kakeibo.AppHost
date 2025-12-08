namespace Kakeibo.AppHost.Web.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserId { get; set; } = "";
    }
}
