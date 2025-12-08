using Kakeibo.AppHost.Web.Models;
using System.Net.Http.Headers;

namespace Kakeibo.AppHost.Web.Services;

public class TokenService
{
    private readonly IHttpClientFactory _clientFactory;
    private static string? _jwtToken;

    public TokenService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    // Login usado pela API
    public async Task<bool> LoginAsync(string email, string password)
    {
        var client = _clientFactory.CreateClient("apis");

        var response = await client.PostAsJsonAsync("/auth/login", new LoginModel
        {
            Email = email,
            Password = password
        });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        _jwtToken = result?.Token;
        return !string.IsNullOrEmpty(_jwtToken);
    }

    // 🔥 MÉTODO QUE VOCÊ ESTÁ TENTANDO USAR
    public Task<bool> BlazorLogin(LoginModel model)
        => LoginAsync(model.Email, model.Password);

    // Adiciona token no header
    public static void AddJwtHeader(HttpClient client)
    {
        if (!string.IsNullOrEmpty(_jwtToken))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    // Cria HttpClient já autenticado
    public HttpClient CreateClient()
    {
        var client = _clientFactory.CreateClient("apis");
        AddJwtHeader(client);
        return client;
    }
}
