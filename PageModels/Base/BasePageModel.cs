using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ElectoralMonitoring
{
	public partial class BasePageModel : ObservableRecipient
    {
        protected readonly AuthService _authService;

        public BasePageModel(AuthService authService)
		{
            _authService = authService;
            AuthService.LoggedOut += AuthService_LoggedOut;

        }

        private void AuthService_LoggedOut(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(nameof(LoginPageModel)));
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        async Task Logout()
        {
            await _authService.LogOut();
        }
    }
}

