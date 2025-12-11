using System.Collections.Generic;

namespace Kakeibo.AppHost.Web.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Email { get; set; } = "";

        public string UserID { get; set; } = "";

        public List<string> Roles { get; set; } = new List<string>();
    }
}