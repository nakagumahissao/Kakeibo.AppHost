using System.Security.Claims;

namespace Kakeibo.AppHost.Web.Services
{
    public class JwtHelper
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var claims = token.Claims.ToList();

            // Map roles
            var roleClaims = claims.Where(c => c.Type == "role").ToList();
            foreach (var r in roleClaims)
                claims.Add(new Claim(System.Security.Claims.ClaimTypes.Role, r.Value));

            return claims;
        }
    }
}
