using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Localization; // REQUIRED for IRequestCultureFeature
using Serilog;
using System.Globalization;

namespace Kakeibo.AppHost.Web
{
    public class CultureCircuitHandler : CircuitHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CultureCircuitHandler(IHttpContextAccessor httpContextAccessor)
        {
            // Injecting IHttpContextAccessor is correct
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task OnCircuitOpenedAsync(
            Circuit circuit,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // 1. Get the RequestCulture set by the middleware
            var requestCultureFeature = httpContext?.Features.Get<IRequestCultureFeature>();

            if (requestCultureFeature is not null)
            {
                var culture = requestCultureFeature.RequestCulture.Culture;
                var uiCulture = requestCultureFeature.RequestCulture.UICulture;

                // Log for verification
                Log.Information(
                    "Circuit opened. Applying culture from HttpContext: Culture = {CultureName}, UICulture = {UICultureName}",
                    culture.Name,
                    uiCulture.Name
                );

                // 2. Apply the correctly determined culture to the Blazor circuit's thread
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = uiCulture;

                // For Blazor Server, setting CurrentCulture/CurrentUICulture is usually enough.
                // You can omit DefaultThreadCurrentCulture/UICulture unless you have other non-Blazor threads.
            }
            else
            {
                // This means the RequestLocalizationMiddleware was not run or failed to set the feature.
                Log.Warning("IRequestCultureFeature not found in HttpContext features.");
            }

            return Task.CompletedTask;
        }
    }
}
