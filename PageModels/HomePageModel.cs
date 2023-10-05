using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ElectoralMonitoring
{
    public partial class HomePageModel : BasePageModel
    {
        [ObservableProperty]
        string username;

        public HomePageModel(AuthService authService) : base(authService)
        {
            Username = _authService.NameUser;
            AuthService.NamedChanged += AuthService_NamedChanged;
        }

        private void AuthService_NamedChanged(object? sender, EventArgs e)
        {
            Username = _authService.NameUser;
        }

        [RelayCommand]
        async Task OpenMonitorList()
        {
            await Shell.Current.GoToAsync(nameof(MonitorListPageModel));
        }
    }
}

