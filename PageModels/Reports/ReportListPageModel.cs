using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MonkeyCache.FileStore;

namespace ElectoralMonitoring
{
    public partial class ReportListPageModel : BasePageModel, IQueryAttributable
    {
        readonly NodeService _nodeService;
        int reportId = 1;
        List<SavedNode> savedNodes;

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
                savedNodes = Barrel.Current.Get<List<SavedNode>>($"{nameof(SavedNode)}/reportes/{AppOption.OptionKey}") ?? new();
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

                //offline saved
                if (savedNodes.Count > 0)
                {
                    Reports ??= new();
                    foreach (var x in savedNodes)
                    {
                        Reports.Add(new DocumentDTO() { Id = x.Id, Title = x.Title, SubTitle = x.SubTitle, Icon = IconFont.FileDocumentAlert });
                    }
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
        public async Task Sync(DocumentDTO doc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;
            doc.Icon = IconFont.FileDocumentRefresh;
            var saved = savedNodes.FirstOrDefault(x => x.Id == doc.Id);
            if (saved is null) return;
            //need post request
            var result = await _nodeService.CreateNode(saved.values, CancellationToken.None);
            if (result != null)
            {
                //success
                savedNodes.Remove(saved);
                Barrel.Current.Add($"{nameof(SavedNode)}/reportes/{AppOption.OptionKey}", savedNodes, TimeSpan.MaxValue);
                await Init().ConfigureAwait(false);
            }
            else
            {
                doc.Icon = IconFont.FileDocumentAlert;
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

