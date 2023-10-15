using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ElectoralMonitoring
{
	public partial class BasePageModel : ObservableRecipient
    {
        protected readonly AuthService _authService;

        [ObservableProperty]
        bool isBusy;

        public BasePageModel(AuthService authService)
		{
            _authService = authService;
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        async Task Logout()
        {
            await _authService.LogOut();
        }
    }
}

