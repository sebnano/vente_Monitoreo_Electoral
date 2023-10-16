using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectoralMonitoring.Resources.Lang;
using MonkeyCache.FileStore;

namespace ElectoralMonitoring
{
    public partial class MonitorListPageModel : BasePageModel
    {
        string ccv;
        string mesa;
        List<VotingCenter> votingCenters;
        List<SavedNode> savedNodes;
        readonly NodeService _nodeService;

        [ObservableProperty]
        ObservableCollection<DocumentDTO>? minutes;

        public MonitorListPageModel(NodeService nodeService, AuthService authService) : base(authService)
        {
            _nodeService = nodeService;
            Minutes ??= new();
            _ = Init();
        }

        public async Task Init()
        {
            if (_authService.IsAuthenticated)
            {
                IsBusy = true;
                Minutes ??= new();
                savedNodes = Barrel.Current.Get<List<SavedNode>>($"{nameof(SavedNode)}/actas") ?? new();
                var list = await _nodeService.GetMinutesByUser(CancellationToken.None);
                if (list != null && list.Count > 0)
                {
                    Minutes = new(list.Select(x => new DocumentDTO()
                    {
                        Title = x.field_centro_de_votacion,
                        SubTitle = x.field_mesa,
                        Id = x.nid,
                        Icon = savedNodes.Any(y=>y.NodeId == x.nid) ? IconFont.FileDocumentAlert : IconFont.FileDocumentCheck
                    }));
                }
                else {
                    Minutes = null;
                }

                //offline saved
                if(savedNodes.Count > 0)
                {
                    Minutes ??= new();
                    foreach (var x in savedNodes)
                    {
                        if (Minutes.Any(minute => minute.Id == x.NodeId))
                            continue;

                        Minutes.Add(new DocumentDTO() { Id = x.Id, Title = x.Title, SubTitle = x.SubTitle, Icon = IconFont.FileDocumentAlert });
                    }
                }
                
                votingCenters = await _nodeService.GetVotingCenters(CancellationToken.None) ?? new();

                IsBusy = false;
            }
        }

        public Microsoft.Maui.Controls.Page? ContextPage { get; set; }

        async Task<bool> CheckCanContinue()
        {
            if(votingCenters != null && votingCenters.Count > 0)
            {
                var hasAccess = votingCenters.Any(x => x.CodCNECentroVotacion == ccv || x.CodCNECentroVotacion == ccv.TrimStart('0'));
                if (!hasAccess) {
                    await Shell.Current.DisplayAlert("Mensaje", $"¡El centro de votación {ccv} no existe o no tiene permisos!", "OK");
                    return false;
                }
            }
            var list = await _nodeService.GetMinutesByCcvAndTable(ccv, mesa, CancellationToken.None);
            if (list != null && list.Count > 0)
            {
                await Shell.Current.DisplayAlert("Mensaje", $"¡El acta del centro de votación {ccv} en la mesa {mesa} ya ha sido cargada!", "OK");
                return false;
            }

            return list is not null;
        }

        [RelayCommand]
        public async Task Sync(DocumentDTO doc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;
            doc.Icon = IconFont.FileDocumentRefresh;
            var saved = savedNodes.FirstOrDefault(x => x.Id == doc.Id);
            if (saved is null) return;
            if (!string.IsNullOrEmpty(saved.NodeId))
            {
                //need patch request
                var nid = saved.values["nid"].FirstOrDefault().Value;
                var result = await _nodeService.EditNode(nid.ToString(), saved.values, CancellationToken.None);
                if (result != null)
                {
                    //success
                    savedNodes.Remove(saved);
                    Barrel.Current.Add($"{nameof(SavedNode)}/actas", savedNodes, TimeSpan.MaxValue);
                    await Init().ConfigureAwait(false);
                }
                else
                {
                    doc.Icon = IconFont.FileDocumentAlert;
                }
            }
            else
            {
                //need post request
                var result = await _nodeService.CreateNode(saved.values, CancellationToken.None);
                if(result != null)
                {
                    //success
                    savedNodes.Remove(saved);
                    Barrel.Current.Add($"{nameof(SavedNode)}/actas", savedNodes, TimeSpan.MaxValue);
                    await Init().ConfigureAwait(false);
                }
                else
                {
                    doc.Icon = IconFont.FileDocumentAlert;
                }
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task AddDoc()
        {
            if (votingCenters is null) return;

            if(votingCenters?.Count > 1)
            {
                ccv = await Shell.Current.DisplayPromptAsync("Código del centro de votación", "Ingrese el código para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "Ejemplo: 010101001", 9, Keyboard.Numeric);
                
            }
            else
            {
                var ccvAssigned = votingCenters?.FirstOrDefault()?.CodCNECentroVotacion;
                ccv = ccvAssigned ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(ccv)) return;

            mesa = await Shell.Current.DisplayPromptAsync("Mesa", "Ingrese el número de mesa para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "Ejemplo: 01", 2, Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(mesa)) return;

            IsBusy = true;
            var can = await CheckCanContinue();
            if (!can)
            {
                IsBusy = false;
                return;
            }


            var navigationParameter = new Dictionary<string, object>
            {
                { "ccv", ccv },
                { "mesa", mesa },
            };
            IsBusy = false;
            await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter).ConfigureAwait(false);
        }
    }
}

