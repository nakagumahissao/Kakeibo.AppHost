using System.Collections.Generic;

namespace Kakeibo.AppHost.Web.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;

        public string UserID { get; set; } = String.Empty;

        public List<string> Roles { get; set; } = new List<string>();
    }
}