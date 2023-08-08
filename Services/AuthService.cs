using System.Threading;

namespace ProRecords
{
	public class AuthService : MobileBaseApiService
    {
        readonly static WeakEventManager _loggedOuteventManager = new();

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

        public string CsrfToken
        {
            get => _preferences.Get(nameof(CsrfToken), string.Empty);
            private set => _preferences.Set(nameof(CsrfToken), value);
        }

        public string LogoutToken
        {
            get => _preferences.Get(nameof(LogoutToken), string.Empty);
            private set => _preferences.Set(nameof(LogoutToken), value);
        }

        public async Task<LoginResponse?> Login(string userName, string password, CancellationToken cancellationToken)
        {
            var loginResult = await AttemptAndRetry_Mobile(async () => {

                return await _authApi.Login(new UserCredentials(userName, password)).ConfigureAwait(false);

            }, cancellationToken);

            if(loginResult != null) {

                CsrfToken = loginResult.CsrfToken;
                LogoutToken = loginResult.LogoutToken;
                IdUser = loginResult.CurrentUser.Uid;
                NameUser = loginResult.CurrentUser.Name;
                IsAuthenticated = true;
            }

            return loginResult;
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

        void OnLoggedOut() => _loggedOuteventManager.HandleEvent(this, EventArgs.Empty, nameof(LoggedOut));

        public async override Task LogOut()
        {
            await AttemptAndRetry_Mobile(async () => {

                await _authApi.Logout(LogoutToken).ContinueWith((_) => {
                    if (_.IsCompletedSuccessfully) {
                        CsrfToken = string.Empty;
                        LogoutToken = string.Empty;
                        IdUser = string.Empty;
                        NameUser = string.Empty;
                        IsAuthenticated = false;
                        OnLoggedOut();
                    }
                }).ConfigureAwait(false);

                return Task.CompletedTask;

            }, CancellationToken.None).ConfigureAwait(false);
        }
    }
}

