using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ElectoralMonitoring
{
    public partial class HomePageModel : BasePageModel
    {
        [ObservableProperty]
        string username;

        [ObservableProperty]
        ObservableCollection<AppOptions> options;

        public HomePageModel(AuthService authService) : base(authService)
        {
            Username = _authService.NameUser;
            AuthService.NamedChanged += AuthService_NamedChanged;
            _=Init();
        }

        async Task Init()
        {
            if (_authService.IsAuthenticated)
            {
                IsBusy = true;
                var opts = await _authService.GetUserOptions();
                Options = new(opts);
                IsBusy = false;
            }
        }

        private void AuthService_NamedChanged(object? sender, EventArgs e)
        {
            Username = _authService.NameUser;
            _ = Init();
        }

        [RelayCommand]
        async Task OpenOption(AppOptions opt)
        {
            //todo define opts navigation
            if(opt.OptionKey == AppOptions.MINUTES_ADD)
            {
                await Shell.Current.GoToAsync(nameof(MonitorListPageModel));
            }
            else if(opt.OptionKey == AppOptions.MINUTES_EDIT)
            {
                //todo pide ccv y mesa a editar, luego navega al formulario del acta
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(ReportListPageModel), new Dictionary<string, object>()
                {
                    { "option", opt }
                });
            }
            
        }
    }
}

