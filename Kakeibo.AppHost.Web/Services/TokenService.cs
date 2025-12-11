using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Kakeibo.AppHost.Web.Services
{
    public class TokenService
    {
        private string? _token;

        public void SetToken(string token) => _token = token;

        public string? GetToken() => _token;

        public string? GetUserIdFromToken()
        {
            if (_token == null) return null;

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_token);
            return jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||  // GUID
                c.Type == "sub"                         // common fallback
            )?.Value;
        }

        // =========================
        // LOGOUT
        // =========================
        public void Logout()
        {
            _token = null;
        }
    }
}
