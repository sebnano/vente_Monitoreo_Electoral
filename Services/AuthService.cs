namespace ProRecords
{
	public class AuthService : MobileBaseApiService
    {
        readonly IAuthApi _authApi;
        readonly IPreferences _preferences;

        public AuthService(AnalyticsService analyticsService, IAuthApi authApi, IPreferences preferences) : base(analyticsService)
        {
            _authApi = authApi;
            _preferences = preferences;
        }

        public bool IsAuthenticated
        {
            get => _preferences.Get(nameof(IsAuthenticated), false);
            private set => _preferences.Set(nameof(IsAuthenticated), value);
        }

        public async Task<object> Login(string userName, string? password, CancellationToken cancellationToken)
        {
            var loginResult = await AttemptAndRetry_Mobile(async () => {

                return await _authApi.Login(userName).ConfigureAwait(false);

            }, cancellationToken);

            return loginResult;
        } 
    }
}

