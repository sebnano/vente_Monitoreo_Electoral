using System;
using CommunityToolkit.Mvvm.Input;

namespace ProRecords
{
	public partial class MainPageModel : BasePageModel
    {
        readonly AuthService _authService;
        public MainPageModel(AuthService authService)
        {
            _authService = authService;
            AuthService.LoggedOut += AuthService_LoggedOut;
        }

        private void AuthService_LoggedOut(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async() => await Shell.Current.GoToAsync(nameof(RegisterPageModel)));            
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
		async Task Logout() {
            await _authService.LogOut();
		}
	}
}

