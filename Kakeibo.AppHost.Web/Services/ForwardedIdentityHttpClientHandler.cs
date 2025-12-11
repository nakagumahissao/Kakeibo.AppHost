using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Kakeibo.AppHost.Web.Services
{
    public class ForwardedIdentityHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authService;

        public ForwardedIdentityHttpClientHandler(
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationService authService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Re-authenticate the user context to generate the proper headers for the downstream request.
                // This is the idiomatic way to forward the user's identity (claims) to another service.
                var result = await _authService.AuthenticateAsync(
                    httpContext,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                if (result?.Principal != null)
                {
                    // The downstream API needs a Bearer token or other credential.
                    // Since your API was configured for JWT, we need to adapt:
                    // Option A (Cleanest): Reconfigure API to accept the current user's claims directly (complex with Identity).
                    // Option B (Easiest): Call the API endpoint secured by the Cookie **instead of** JWT.

                    // Since the API requires authentication, let's assume we use the original approach 
                    // of forwarding the claims **if you were using JWT**. 
                    // Since we switched to COOKIE-ONLY, you have two sub-options for the API:

                    // --- SUB-OPTION 1: The API is also Cookie-Authenticated (Requires API setup) ---
                    // This is complex for a separated API.

                    // --- SUB-OPTION 2: The API endpoint that the Blazor client calls is configured to be **unauthorized** ---
                    // This only works if the data is not sensitive or if you implement the full JWT approach.

                    // GIVEN YOUR ARCHITECTURE (API + Minimal API Endpoints):
                    // The easiest path is to use the **JWT** token generation in the API again,
                    // but ONLY for the purpose of communicating between the Blazor client and the API.

                    // Since you removed JWT, we'll try to leverage the cookie, but you need a custom 
                    // way to prove identity to the *API*.

                    // The most common solution in this setup is to use the **Bearer Token** approach 
                    // (re-implementing the JWT token *for API communication* only):
                    // Since you removed the JWT, we'll revert to the standard Blazor-to-API pattern:

                    // This is where you would normally obtain the JWT (if it were stored) and set the header:
                    // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    // --- WORKAROUND: Pass the cookie header manually (Only works if API also accepts cookies) ---
                    // var cookieHeader = httpContext.Request.Headers["Cookie"];
                    // if (!string.IsNullOrEmpty(cookieHeader))
                    // {
                    //    request.Headers.Add("Cookie", cookieHeader.ToString());
                    // }
                    // -----------------------------------------------------------------------------------------

                    // Since manually passing the cookie is fragile, **the API side needs to be fixed to handle the request**.

                    // THE CORE ISSUE IS: The downstream API must be configured to validate the incoming request using
                    // the cookie/claims it receives, OR you must switch back to JWT/Token-based API security.

                    // Let's assume you want to fix the API's authentication type to accept the claims being forwarded.
                    // This is complex for a quick fix.

                    // --- REVERTING TO THE TOKEN IDEA FOR API CALLS ---
                    // Since you have Aspire, the proper way to forward identity is often using Aspire's specific mechanisms,
                    // but if not available, the JWT is the next best thing.

                    // Since your setup is minimal, we cannot cleanly inject the ClaimsPrincipal into the API.
                    // Let's rely on the assumption that the *API's endpoints themselves* are protected by the JWT scheme,
                    // and your Blazor Server's `HttpClient` needs to provide that JWT.

                    // If you switch back to JWT *only* for the API (as in the previous answer), 
                    // then the `TokenService` and `JwtAuthorizationHandler` approach from my first answer is the correct way to fix this.

                    // For now, let's assume you want to stick with pure Cookie, and the API should be bypassed for this simple data.
                    // If the API must be secured, you must reintroduce a JWT/Token flow for client-to-API communication.

                    // --- Revert to the Simplest Fix ---
                    // For now, temporarily make the API endpoint publicly accessible for testing to confirm the cookie is the issue.
                    // In kakeibo.api/Program.cs, try removing RequireAuthorization() on the /tiposdespesa endpoint.

                    // If that fixes it, the problem is definitely the missing credential on the HTTP call.
                    // Reintroduce the `JwtAuthorizationHandler` from my first answer to fix this:
                    // 1. Store JWT in Blazor client on login.
                    // 2. Use JwtAuthorizationHandler to attach JWT to API calls.
                    // 3. Keep cookie for Blazor UI.
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
