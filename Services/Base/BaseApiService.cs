using Polly;
using Refit;
using Fusillade;
using System.ComponentModel;

namespace ProRecords
{
    public abstract class BaseApiService
    {
        static HttpClient CreateHttpClient(in HttpMessageHandler httpMessageHandler)
        {
            HttpClient client;
            if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android)
            {
                client = new HttpClient(httpMessageHandler);
            }
            else
            {
                client = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });
            }

            client.BaseAddress = new Uri(Helpers.AppSettings.BackendUrl);
            client.Timeout = TimeSpan.FromSeconds(9999);
            return client;
        }

        static HttpClient Background
        {
            get
            {
                return new Lazy<HttpClient>(() => CreateHttpClient(
                    new RateLimitedHttpMessageHandler(new HttpClientHandler(), Priority.Background))).Value;
            }
        }

        static HttpClient UserInitiated
        {
            get
            {
                return new Lazy<HttpClient>(() => CreateHttpClient(
              new RateLimitedHttpMessageHandler(new HttpClientHandler(), Priority.UserInitiated))).Value;
            }
        }

        static HttpClient Speculative
        {
            get
            {
                return new Lazy<HttpClient>(() => CreateHttpClient(
              new RateLimitedHttpMessageHandler(new HttpClientHandler(), Priority.Speculative))).Value;
            }
        }

        public static HttpClient GetApi(Priority priority)
        {
            switch (priority)
            {
                case Priority.Background:
                    return Background;
                case Priority.UserInitiated:
                    return UserInitiated;
                case Priority.Speculative:
                    return Speculative;
                default:
                    return UserInitiated;
            }
        }

        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3)
        {
            var onRetryInner = new Func<Exception, TimeSpan, Task>((e, t) =>
            {
                return Task.Factory.StartNew(() =>
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"Retrying in {t.ToString("g")}");
                    if (e is ApiException apiException)
                    {
                        System.Diagnostics.Debug.WriteLine($"URI: {apiException.Uri?.ToString()}");
                        System.Diagnostics.Debug.WriteLine($"Content: {apiException.Content}");
                    }
                    System.Diagnostics.Debug.WriteLine($"Exception '{(e.InnerException ?? e).Message}'");
#endif
                });
            });

            return Policy
            .Handle<Exception>(shouldHandleException)
            .WaitAndRetryAsync(numRetries, retryAttempt, onRetryInner)
            .ExecuteAsync<T>(token => action(), cancellationToken);
            static TimeSpan retryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));

            static bool shouldHandleException(Exception exception)
            {
                if (exception is ApiException apiException)
                {
                    return !is404Or401or403(apiException);
                }

                return true;

                static bool is404Or401or403(ApiException apiException) => apiException.StatusCode is System.Net.HttpStatusCode.Forbidden || apiException.StatusCode is System.Net.HttpStatusCode.Unauthorized || apiException.StatusCode is System.Net.HttpStatusCode.NotFound;
            }
        }

        public static bool ExceptionContainsErrorCode(Exception e, params int[] errorCode)
        {
            if (e is Win32Exception wex)
                return errorCode.Contains(wex.ErrorCode);

            if (e.InnerException != null)
                return ExceptionContainsErrorCode(e.InnerException, errorCode);

            return false;
        }
    }
}

