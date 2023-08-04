using System;
using System.Threading.Tasks;
using Refit;

namespace ProRecords
{
    [Headers("Accept: application/json", "Accept-Encoding: gzip, deflate, br", "User-Agent: " + nameof(ProRecords) + "App", "")]
    public interface IAuthApi
    {
        [Post("/seguridad/autenticar")]
        Task<object> Login([Body] object credentials);
    }
}

