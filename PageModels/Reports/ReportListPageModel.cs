using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ElectoralMonitoring
{
    public partial class ReportListPageModel : BasePageModel, IQueryAttributable
    {
        readonly NodeService _nodeService;
        int reportId = 1;

        [ObservableProperty]
        AppOptions appOption;

        [ObservableProperty]
        ObservableCollection<DocumentDTO>? reports;

        public ReportListPageModel(AuthService authService, NodeService nodeService) : base(authService)
        {
            _nodeService = nodeService;
        }

        public async Task Init()
        {
            if (_authService.IsAuthenticated)
            {
                IsBusy = true;
                Reports ??= new();
                var list = await _nodeService.GetUserReports(reportId.ToString(), CancellationToken.None);
                if (list != null && list.Count > 0)
                {
                    Reports = new(list.Select(x => new DocumentDTO()
                    {
                        Title = x.Title,
                        SubTitle = string.Empty,
                        Id = x.Nid,
                        Icon = IconFont.FileDocumentCheck
                    }));
                }
                else
                {
                    Reports = null;
                }

                IsBusy = false;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("option"))
            {
                AppOption = query["option"] as AppOptions;
                if(AppOption.OptionKey == AppOptions.REPORT_OPENED)
                {
                    reportId = 1;
                }else if(AppOption.OptionKey == AppOptions.REPORT_SCHEDULED)
                {
                    reportId = 4;
                }
                else if (AppOption.OptionKey == AppOptions.REPORT_INCIDENCE)
                {
                    reportId = 3;
                }
                _ = Init();
            }
        }

        [RelayCommand]
        async Task Add()
        {
            await Shell.Current.GoToAsync(nameof(ReportFormPageModel), new Dictionary<string, object>()
            {
                { "option", AppOption }
            });
        }
    }
}

