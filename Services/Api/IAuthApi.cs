using System;
using System.Threading.Tasks;
using Refit;

namespace ElectoralMonitoring
{
    [Headers("Accept: application/json", "Content-Type: application/json", "Accept-Encoding: gzip, deflate, br", "User-Agent: " + nameof(ElectoralMonitoring) + "App", "")]
    public interface IAuthApi
    {
        [Post("/oauth/token")]
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        Task<TokenResponse> OAuth2Token([Body(BodySerializationMethod.UrlEncoded)] ClientCredentials credentials);

        [Post("/oauth/token?refresh=true")]
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        Task<TokenResponse> OAuth2TokenRefresh([Body(BodySerializationMethod.UrlEncoded)] RefreshTokenCredentials credentials);

        [Post("/user/login?_format=json")]
        Task<LoginResponse> Login([Body] UserCredentials credentials);

        [Post("/user/register?_format=json")]
        Task<User> Register([Body] UserRegister user);

        [Get("/user/{userId}?_format=json")]
        Task<User> GetUser([AliasAs("userId")] string userId);

        [Get("/role-by-user/{userId}?_format=json")]
        Task<List<UserRoles>> GetUserRoles([AliasAs("userId")] string userId);

        [Get("/api-opciones-paginas/page_option_home_page?_format=json")]
        Task<List<AppOptions>> GetHomeOptions();
    }
}

