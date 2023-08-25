using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectoralMonitoring.Resources.Lang;

namespace ElectoralMonitoring
{
	public partial class RegisterPageModel : BasePageModel
    {
        [ObservableProperty]
		string username = string.Empty;
        [ObservableProperty]
        string email = string.Empty;
        [ObservableProperty]
        string password = string.Empty;
        [ObservableProperty]
        string confirmPassword = string.Empty;

        public RegisterPageModel(AuthService authService) : base(authService)
		{
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                await Shell.Current.DisplayAlert(AppRes.AlertTitle, AppRes.AlertUsernamesEmpty, AppRes.AlertAccept);
                return;
            }else if (string.IsNullOrWhiteSpace(Email))
            {
                await Shell.Current.DisplayAlert(AppRes.AlertTitle, AppRes.AlertEmailEmpty, AppRes.AlertAccept);
                return;
            }else if (string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert(AppRes.AlertTitle, AppRes.AlertPasswordEmpty, AppRes.AlertAccept);
                return;
            }else if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Shell.Current.DisplayAlert(AppRes.AlertTitle, AppRes.AlertConfirmPassEmpty, AppRes.AlertAccept);
                return;
            }else if (Password != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert(AppRes.AlertTitle, AppRes.AlertPasswordsNotMatch, AppRes.AlertAccept);
                return;
            }

            var result = await _authService.Register(Username, Email, Password, CancellationToken.None);
            if (result != null)
            {
                await Shell.Current.GoToAsync($"{nameof(LoginPageModel)}?username={Username}&password={Password}");
            }
        }
    }
}

