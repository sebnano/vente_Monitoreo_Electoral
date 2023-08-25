using System.Net;
using System.Runtime.CompilerServices;
using ElectoralMonitoring.Resources.Lang;
using Refit;

namespace ElectoralMonitoring
{
    public abstract class MobileBaseApiService : BaseApiService
    {

        protected MobileBaseApiService(AnalyticsService analyticsService) =>
            (AnalyticsService) = (analyticsService);

        protected AnalyticsService AnalyticsService { get; }

        protected async Task<T?> AttemptAndRetry_Mobile<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3, IDictionary<string, object>? properties = null, [CallerMemberName] string callerName = "")
        {
            try
            {
                using var timedEvent = AnalyticsService.TrackTime(callerName, properties);
                return await AttemptAndRetry(action, cancellationToken, numRetries).ConfigureAwait(false);
            }
            catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized)
            {
                AnalyticsService.Track(ex.Message);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"URI: {ex.Uri?.ToString()}");
                System.Diagnostics.Debug.WriteLine($"Content: {ex.Content}");
#endif
                AnalyticsService.Track("force_logout");
                await LogOut().ConfigureAwait(false);
                return default(T);
            }
            catch (ApiException ex) when (!shouldHandleException(ex))
            {
                AnalyticsService.Track(ex.Message);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"URI: {ex.Uri?.ToString()}");
                System.Diagnostics.Debug.WriteLine($"Content: {ex.Content}");
#endif
                try
                {
                    var response = await ex.GetContentAsAsync<ServerResponse>().ConfigureAwait(false);
                    if(response != null && !string.IsNullOrWhiteSpace(response.Message)) {
                        MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.DisplayAlert(AppRes.AlertTitle, response.Message, AppRes.AlertAccept));
			        }
                }
                catch
                {
                    MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.DisplayAlert(AppRes.AlertTitle, AppRes.AlertErrorGeneral, AppRes.AlertAccept));
                }
                return default(T);
            }
            catch (Exception ex)
            {
                AnalyticsService.Report(ex);
                MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.DisplayAlert(AppRes.AlertTitle, AppRes.AlertErrorGeneral, AppRes.AlertAccept));
                return default(T);
            }
            finally { }
        }

        public abstract Task LogOut();
    }
}

