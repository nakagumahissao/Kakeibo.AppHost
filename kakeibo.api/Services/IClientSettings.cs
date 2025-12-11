using kakeibo.api.Data;
using Microsoft.Extensions.Options;

namespace kakeibo.api.Services
{
    public interface IClientSettings
    {
        ClientSettings GetClientSettings();
    }

    public class ClientSettingsService : IClientSettings
    {
        private readonly ClientSettings _settings;

        public ClientSettingsService(IOptions<ClientSettings> options)
        {
            _settings = options.Value;
        }

        public ClientSettings GetClientSettings()
        {
            return _settings;
        }
    }
}
