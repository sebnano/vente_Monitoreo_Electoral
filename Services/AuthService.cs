﻿using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using ElectoralMonitoring.Helpers;
using MonkeyCache.FileStore;
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

        public async Task<User?> GetCurrentUser(CancellationToken cancellationToken, bool forceRefresh = false)
        {
            var currentUserResult = await AttemptAndRetry_Mobile(async () => {

                User? user = null;
                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                    user = Barrel.Current.Get<User>(nameof(GetCurrentUser));
                else if (!refresh && !Barrel.Current.IsExpired(nameof(GetCurrentUser)))
                    user = Barrel.Current.Get<User>(nameof(GetCurrentUser));

                if (user != null) return user;

                user = await _authApi.GetUser(IdUser).ConfigureAwait(false);
                Barrel.Current.Add(nameof(GetCurrentUser), user, TimeSpan.FromSeconds(cacheSeconds));
                return user;

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
            Barrel.Current.EmptyAll();
            OnLoggedOut();
            OnNameChanged();
            return Task.CompletedTask;
        }

        public async Task<string> GetAccessToken()
        {
            try
            {
                if (!string.IsNullOrEmpty(AccessToken))
                {
                    var token = new JwtSecurityToken(AccessToken);
                    if (token.ValidTo < DateTime.UtcNow)
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
                }
            }
            catch (Exception ex)
            {
                AnalyticsService.Report(ex);
            }
            

            return AccessToken;
        }

        public Task<List<string>?> GetUserRoles(bool forceRefresh = false) => AttemptAndRetry_Mobile(async () =>
        {
            List<string>? opts = null;
            var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                opts = Barrel.Current.Get<List<string>>(nameof(GetUserRoles));
            else if (!refresh && !Barrel.Current.IsExpired(nameof(GetUserRoles)))
                opts = Barrel.Current.Get<List<string>>(nameof(GetUserRoles));

            if (opts != null) return opts;

            var rolesUser = await _authApi.GetUserRoles(IdUser);
            return rolesUser.Select(x => x.Role).ToList();

        }, CancellationToken.None);

        public Task<List<AppOptions>?> GetUserOptions(bool forceRefresh = false) => AttemptAndRetry_Mobile(async() =>
        {
            List<AppOptions>? opts = null;
            var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                opts = Barrel.Current.Get<List<AppOptions>>(nameof(GetUserOptions));
            else if (!refresh && !Barrel.Current.IsExpired(nameof(GetUserOptions)))
                opts = Barrel.Current.Get<List<AppOptions>>(nameof(GetUserOptions));

            if (opts != null) return opts;

            var roles = await GetUserRoles(true);
            var options = await _authApi.GetHomeOptions();
            var filtered = options.Where(option =>
            {
                var rolOpts = option.Rol.Split(",");
                var options = 0;
                foreach (var roleOpt in rolOpts)
                {
                    var roleScape = roleOpt.TrimStart().TrimEnd();
                    if (roles?.Contains(roleScape) == true)
                    {
                        options++;
                    }
                }
                return options > 0;
            });
            opts = filtered.ToList();
            Barrel.Current.Add(nameof(GetUserOptions), opts, TimeSpan.FromSeconds(cacheSeconds));
            return opts;
        }, CancellationToken.None);

        public Task<List<AppConfig>?> GetAppConfig(bool forceRefresh = false) => AttemptAndRetry_Mobile(async () =>
        {
            List<AppConfig>? opts = null;

            var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
            
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                opts = Barrel.Current.Get<List<AppConfig>>(nameof(GetAppConfig));
            else if (!refresh && !Barrel.Current.IsExpired(nameof(GetAppConfig)))
                opts = Barrel.Current.Get<List<AppConfig>>(nameof(GetAppConfig));

            if (opts != null) return opts;

            opts = await _authApi.GetConfiguration();
            Barrel.Current.Add(nameof(GetAppConfig), opts, TimeSpan.FromSeconds(cacheSeconds));
            return opts;

        }, CancellationToken.None);

        double cacheSeconds = 14400;
    }
}

