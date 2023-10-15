using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ElectoralMonitoring
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        AuthService? _authService;

        public AuthHeaderHandler()
        {
            _authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
            InnerHandler = new HttpLoggingHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(_authService is null)
            {
                _authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
            }

            if(_authService is not null)
            {
                string token = await _authService?.GetAccessToken();

                //potentially refresh token here if it has expired etc.
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}

