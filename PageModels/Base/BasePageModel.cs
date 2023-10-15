using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectoralMonitoring.Resources.Lang;

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
            var opt = await Shell.Current.DisplayAlert(AppRes.AlertTitle, "¿Desea cerrar sesión?", "SI", "NO");
            if(opt)
                await _authService.LogOut();
        }
    }
}

