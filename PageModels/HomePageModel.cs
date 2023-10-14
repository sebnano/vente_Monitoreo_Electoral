using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectoralMonitoring.Resources.Lang;

namespace ElectoralMonitoring
{
    public partial class HomePageModel : BasePageModel
    {
        readonly NodeService _nodeService;
        [ObservableProperty]
        string username;

        [ObservableProperty]
        ObservableCollection<AppOptions> options;

        public HomePageModel(NodeService nodeService, AuthService authService) : base(authService)
        {
            _nodeService = nodeService;
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
                await EditDoc();
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(ReportListPageModel), new Dictionary<string, object>()
                {
                    { "option", opt }
                });
            }
            
        }

        public async Task EditDoc()
        {
            var ccv = await Shell.Current.DisplayPromptAsync("Código del centro de votación", "Ingrese el código para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "Ejemplo: 010101001", 9, Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(ccv)) return;

            var mesa = await Shell.Current.DisplayPromptAsync("Mesa", "Ingrese el número de mesa para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "Ejemplo: 01", 2, Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(mesa)) return;

            IsBusy = true;

            var list = await _nodeService.GetMinutesByCcvAndTable(ccv, mesa, CancellationToken.None);
            if (list != null && list.Count > 0)
            {

                var navigationParameter = new Dictionary<string, object>
                {
                    { "ccv", ccv },
                    { "mesa", mesa },
                    { "nodeId", list.FirstOrDefault().nid }
                };
                await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter).ConfigureAwait(false);
            }
            IsBusy = false;
        }
    }
}

