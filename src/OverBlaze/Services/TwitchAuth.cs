using System;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;

namespace OverBlaze.Services
{
    public class TwitchAuth
    {
        private const string ClientId = "7mtekewyfkadmhxarxyrektpsrb9vq";
        
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private string? _token;

        public TwitchAuth(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool TrySetTokens(string? idToken, string? accessToken)
        {
            if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(accessToken)) return false;
            
            if (TryParseJsonPayload(idToken, out var json))
            {
                if (json.RootElement.TryGetProperty("exp", out var expProperty)
                    && expProperty.TryGetInt64(out long exp))
                {
                    if (DateTimeOffset.FromUnixTimeSeconds(exp) < DateTimeOffset.UtcNow)
                    {
                        return false;
                    }
                }
                if (json.RootElement.TryGetProperty("preferred_username", out var usernameProperty))
                {
                    UserName = usernameProperty.GetString();
                }
            }
            Token = accessToken;
            TokenSet?.Invoke();
            return true;
        }

        private static bool TryParseJsonPayload(string idToken, out JsonDocument json)
        {
            var parts = idToken.Split('.');
            json = default;
            if (parts.Length < 3) return false;
            var payload = parts[1];
            while (payload.Length % 4 != 0)
            {
                payload += '=';
            }

            var bytes = Convert.FromBase64String(payload);
            var text = Encoding.UTF8.GetString(bytes);
            json = JsonDocument.Parse(text);
            return true;
        }

        public string? UserName { get; private set; }
        public string? Token { get; private set; }

        public string GetAuthenticationUri()
        {
            var redirectUri = _configuration.GetValue<string>("Twitch:RedirectUri")
                              ?? "http://localhost:25293/twitch_callback.html";
            redirectUri = Uri.EscapeUriString(redirectUri);

            return $"https://id.twitch.tv/oauth2/authorize?client_id={ClientId}" +
                   $"&redirect_uri={redirectUri}&response_type=token+id_token" +
                   "&scope=openid+chat:edit+chat:read";
        }

        public event Action TokenSet;
    }
}