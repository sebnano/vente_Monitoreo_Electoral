using System;
using System.Threading.Tasks;
using Refit;

namespace ProRecords
{
    [Headers("Accept: application/json", "Content-Type: application/json", "Accept-Encoding: gzip, deflate, br", "User-Agent: " + nameof(ProRecords) + "App", "")]
    public interface IAuthApi
    {
        [Post("/user/login?_format=json")]
        Task<LoginResponse> Login([Body] UserCredentials credentials);

        [Post("/user/register?_format=json")]
        Task<User> Register([Body] UserRegister user);

        [Post("/user/logout?_format=json")]
        Task Logout([AliasAs("token")] string token);
    }
}

