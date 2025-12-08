using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Kakeibo.AppHost.Web
{
    public class CustomAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;

        public CustomAuthorizationMessageHandler(IJSRuntime js)
        {
            _js = js;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Lê o JWT gravado no localStorage
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
