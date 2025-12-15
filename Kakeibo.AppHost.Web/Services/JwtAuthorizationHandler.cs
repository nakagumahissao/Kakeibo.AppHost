using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Kakeibo.AppHost.Web.Services
{
    // JwtAuthorizationHandler inherits from DelegatingHandler
    public class JwtAuthorizationHandler : DelegatingHandler
    {
        private readonly TokenService _tokenService;
        private readonly ILogger<JwtAuthorizationHandler> _logger;

        // Dependency Injection automatically provides the TokenService
        public JwtAuthorizationHandler(TokenService tokenService, ILogger<JwtAuthorizationHandler> Logger)
        {
            _tokenService = tokenService;
            _logger = Logger;
        }

        // This method is executed before the HTTP request is sent
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var jwtToken = _tokenService.GetToken();

            _logger.LogInformation("Attaching JWT Token to request: {Token}", jwtToken);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                // Attach the JWT as a Bearer token to the request header
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            // Pass the request down the pipeline
            return base.SendAsync(request, cancellationToken);
        }
    }
}