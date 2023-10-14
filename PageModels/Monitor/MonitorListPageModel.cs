using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectoralMonitoring.Resources.Lang;
using Microsoft.Maui.Graphics.Platform;
using Plugin.Firebase.Functions;
using Plugin.Firebase.Storage;

namespace ElectoralMonitoring
{
    public partial class MonitorListPageModel : BasePageModel
    {
        string ccv;
        string mesa;
        List<VotingCenter> votingCenters;

        readonly NodeService _nodeService;

        [ObservableProperty]
        bool isAdding;

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
                var list = await _nodeService.GetMinutesByUser(CancellationToken.None);
                if (list != null && list.Count > 0)
                {
                    Minutes = new(list.Select(x => new DocumentDTO()
                    {
                        Title = x.field_centro_de_votacion,
                        SubTitle = x.field_mesa,
                        Id = x.nid,
                        Icon = IconFont.FileDocumentCheck
                    }));
                }
                else {
                    Minutes = null;
                }
                votingCenters = await _nodeService.GetVotingCenters(CancellationToken.None) ?? new();

                await Task.Yield();

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

        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task AddDoc()
        {
            if(votingCenters.Count > 1)
            {
                ccv = await Shell.Current.DisplayPromptAsync("Código del centro de votación", "Ingrese el código para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "Ejemplo: 010101001", 9, Keyboard.Numeric);
                
            }
            else
            {
                var ccvAssigned = votingCenters.SingleOrDefault()?.CodCNECentroVotacion;
                ccv = ccvAssigned ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(ccv)) return;

            mesa = await Shell.Current.DisplayPromptAsync("Mesa", "Ingrese el número de mesa para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "Ejemplo: 01", 2, Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(mesa)) return;

            IsAdding = true;
            var can = await CheckCanContinue();
            if (!can)
            {
                IsAdding = false;
                return;
            }


            var navigationParameter = new Dictionary<string, object>
            {
                { "ccv", ccv },
                { "mesa", mesa },
            };
            IsAdding = false;
            await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter).ConfigureAwait(false);
        }
    }
}

