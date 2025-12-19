using Microsoft.AspNetCore.Localization;

namespace kakeibo.api.Services
{
    public class JwtCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var user = httpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return Task.FromResult<ProviderCultureResult?>(null);

            var cultureClaim = user.FindFirst("culture")?.Value;

            if (string.IsNullOrWhiteSpace(cultureClaim))
                return Task.FromResult<ProviderCultureResult?>(null);

            return Task.FromResult<ProviderCultureResult?>(
                new ProviderCultureResult(cultureClaim, cultureClaim)
            );
        }
    }

}
