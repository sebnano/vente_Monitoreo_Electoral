using System.Runtime.CompilerServices;

namespace ProRecords
{
    public class MobileBaseApiService : BaseApiService
    {

        protected MobileBaseApiService(AnalyticsService analyticsService) =>
            (AnalyticsService) = (analyticsService);

        protected AnalyticsService AnalyticsService { get; }

        protected string GetTokenHeader(string token) => $"{token}";

        protected async Task<T> AttemptAndRetry_Mobile<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3, IDictionary<string, object>? properties = null, [CallerMemberName] string callerName = "")
        {
            try
            {
                using var timedEvent = AnalyticsService.TrackTime(callerName, properties);
                return await AttemptAndRetry(action, cancellationToken, numRetries).ConfigureAwait(false);
            }
            finally { }
        }
    }
}

