using System;
using System.Threading.Tasks;
using Refit;

namespace ElectoralMonitoring
{
    [Headers("Accept: application/json", "Content-Type: application/json", "Accept-Encoding: gzip, deflate, br", "User-Agent: " + nameof(ElectoralMonitoring) + "App", "")]
    public interface IAuthApi
    {
        [Post("/oauth/token")]
        Task<TokenResponse> OAuth2Token([Body(BodySerializationMethod.UrlEncoded)] ClientCredentials credentials);

        [Post("/oauth/token?refresh")]
        Task<TokenResponse> OAuth2TokenRefresh([Body(BodySerializationMethod.UrlEncoded)] RefreshTokenCredentials credentials);

        [Post("/user/login?_format=json")]
        Task<LoginResponse> Login([Body] UserCredentials credentials);

        [Post("/user/register?_format=json")]
        Task<User> Register([Body] UserRegister user);

        [Get("/user/{userId}?_format=json")]
        Task<User> GetUser(string csrfToken, [AliasAs("userId")] string userId);
    }
}

