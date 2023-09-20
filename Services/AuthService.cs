using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using ElectoralMonitoring.Helpers;
using Plugin.Firebase.Auth;

namespace ElectoralMonitoring
{
	public class AuthService : MobileBaseApiService
    {
        readonly static WeakEventManager _loggedOuteventManager = new();
        readonly static WeakEventManager _nameChangedEventManager = new();

        readonly IAuthApi _authApi;
        readonly IPreferences _preferences;

        public AuthService(AnalyticsService analyticsService, IAuthApi authApi, IPreferences preferences) : base(analyticsService)
        {
            _authApi = authApi;
            _preferences = preferences;
        }

        public static event EventHandler LoggedOut
        {
            add => _loggedOuteventManager.AddEventHandler(value);
            remove => _loggedOuteventManager.RemoveEventHandler(value);
        }

        public static event EventHandler NamedChanged
        {
            add => _nameChangedEventManager.AddEventHandler(value);
            remove => _nameChangedEventManager.RemoveEventHandler(value);
        }

        public bool IsAuthenticated
        {
            get => _preferences.Get(nameof(IsAuthenticated), false);
            private set => _preferences.Set(nameof(IsAuthenticated), value);
        }

        public string IdUser
        {
            get => _preferences.Get(nameof(IdUser), string.Empty);
            private set => _preferences.Set(nameof(IdUser), value);
        }

        public string NameUser
        {
            get => _preferences.Get(nameof(NameUser), string.Empty);
            private set => _preferences.Set(nameof(NameUser), value);
        }

        public string AccessToken
        {
            get => _preferences.Get(nameof(AccessToken), string.Empty);
            private set => _preferences.Set(nameof(AccessToken), value);
        }

        public string RefreshToken
        {
            get => _preferences.Get(nameof(RefreshToken), string.Empty);
            private set => _preferences.Set(nameof(RefreshToken), value);
        }

        public async Task<TokenResponse?> Login(string userName, string password, CancellationToken cancellationToken)
        {
            var tokenResponse = await AttemptAndRetry_Mobile(async () => {

                return await _authApi.OAuth2Token(new ClientCredentials("password", AppSettings.ClientId, AppSettings.ClientSecret, userName, password)).ConfigureAwait(false);

            }, cancellationToken);

            var firAuth = await CrossFirebaseAuth.Current.SignInAnonymouslyAsync();

            if (tokenResponse != null && firAuth != null) {

                var token = new JwtSecurityToken(tokenResponse.AccessToken);
                
                AccessToken = tokenResponse.AccessToken;
                RefreshToken = tokenResponse.RefreshToken;
                IdUser = token.Subject;
                NameUser = userName;
                IsAuthenticated = true;
                OnNameChanged();
            }

            return tokenResponse;
        }

        public async Task<User?> Register(string name, string email, string password, CancellationToken cancellationToken)
        {
            var registerResult = await AttemptAndRetry_Mobile(async () => {

                return await _authApi.Register(new UserRegister(
                    Name: new List<Field>() { new Field(name) },
                    Mail: new List<Field>() { new Field(email) },
                    Pass: new List<Field>() { new Field(password) }
                    )).ConfigureAwait(false);

            }, cancellationToken);

            return registerResult;
        }

        public async Task<User?> GetCurrentUser(CancellationToken cancellationToken)
        {
            var currentUserResult = await AttemptAndRetry_Mobile(async () => {

                return await _authApi.GetUser(await GetAccessToken(), IdUser).ConfigureAwait(false);

            }, cancellationToken);

            return currentUserResult;
        }

        void OnLoggedOut() => _loggedOuteventManager.HandleEvent(this, EventArgs.Empty, nameof(LoggedOut));

        void OnNameChanged() => _nameChangedEventManager.HandleEvent(this, EventArgs.Empty, nameof(NamedChanged));

        public override Task LogOut()
        {
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
            IdUser = string.Empty;
            NameUser = string.Empty;
            IsAuthenticated = false;
            OnLoggedOut();
            OnNameChanged();
            return Task.CompletedTask;
        }

        public async Task<string> GetAccessToken()
        {
            var token = new JwtSecurityToken(AccessToken);
            if(token.ValidTo < DateTime.Now)
            {
                var tokenResponse = await AttemptAndRetry_Mobile(async () => {

                    return await _authApi.OAuth2TokenRefresh(new RefreshTokenCredentials("refresh_token", AppSettings.ClientId, AppSettings.ClientSecret, RefreshToken)).ConfigureAwait(false);

                }, CancellationToken.None);

                if (tokenResponse != null)
                {
                    AccessToken = tokenResponse.AccessToken;
                    RefreshToken = tokenResponse.RefreshToken;
                    IsAuthenticated = true;
                }

            }

            return AccessToken;
        }

    }
}

