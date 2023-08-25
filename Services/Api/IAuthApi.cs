using System;
using System.Threading.Tasks;
using Refit;

namespace ElectoralMonitoring
{
    [Headers("Accept: application/json", "Content-Type: application/json", "Accept-Encoding: gzip, deflate, br", "User-Agent: " + nameof(ElectoralMonitoring) + "App", "")]
    public interface IAuthApi
    {
        [Post("/user/login?_format=json")]
        Task<LoginResponse> Login([Body] UserCredentials credentials);

        [Post("/user/register?_format=json")]
        Task<User> Register([Body] UserRegister user);

        [Post("/user/logout?_format=json")]
        Task Logout([Header("X-CSRF-Token")] string csrfToken, [AliasAs("token")] string token);

        [Get("/user/{userId}?_format=json")]
        Task<User> GetUser([Header("X-CSRF-Token")] string csrfToken, [AliasAs("userId")] string userId);
    }
}

