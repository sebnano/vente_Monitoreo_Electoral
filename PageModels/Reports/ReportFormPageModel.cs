using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MonkeyCache.FileStore;

namespace ElectoralMonitoring
{
    public partial class ReportFormPageModel : BasePageModel, IQueryAttributable
    {
        readonly NodeService _nodeService;
        List<FieldForm>? _form;
        [ObservableProperty]
        AppOptions appOption;

        [ObservableProperty]
        ObservableCollection<View> fields;

        public ReportFormPageModel(NodeService nodeService, AuthService authService) : base(authService)
        {
            _nodeService = nodeService;
        }

        private async Task RenderForm()
        {
            _form = await _nodeService.GetReportForm(AppOption.ContentType, CancellationToken.None);
            Fields ??= new();
            var fieldsObjsToView = _form?.Where(x => x.Active && !x.Hidden).ToList();

            if (fieldsObjsToView != null)
            {
                if(fieldsObjsToView.Any(x => !string.IsNullOrWhiteSpace(x.Grupo)))
                {
                    var grouped = fieldsObjsToView.GroupBy(x => x.Grupo);
                    await Shell.Current.Dispatcher.DispatchAsync(() =>
                    {
                        foreach (var group in grouped)
                        {
                            Label groupTitle = new Label() { Text = group.Key, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 20, 0, 0), FontSize = 16 };
                            Fields.Add(groupTitle);
                            foreach (var item in group.OrderBy(x => x.Weight))
                            {
                                AddFormControlToView(item);
                            }

                        }
                    }).ConfigureAwait(false);
                }
                else
                {
                    await Shell.Current.Dispatcher.DispatchAsync(() =>
                    {
                        foreach (var item in fieldsObjsToView.OrderBy(x => x.Weight))
                        {
                            AddFormControlToView(item);
                        }
                    }).ConfigureAwait(false);
                }
            }
        }

        private void AddFormControlToView(FieldForm? item)
        {
            if (item is null) return;
            if (InputFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                var field = new InputFieldControl()
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key,
                    FieldType = item.Type == FieldForm.NUMBER ? FieldType.Number : FieldType.Text,
                    MaxLenght = item.Key == "field_observaciones" ? -1 : 100,
                    IsRequiredField = item.Required
                };
                if(field.GetKey() == "field_reporte_mesa")
                {
                    MessagingCenter.Subscribe<CheckBoxFieldControl>(field, "CheckBoxFieldControlChanged", (sender) =>
                    {
                        Debug.WriteLine("Receive message CheckBoxFieldControlChanged " + sender.GetKey());
                        if (sender.GetKey() == "field_report_todas_las_mesas" && field.GetKey() == "field_reporte_mesa")
                        {
                            field.IsVisible = sender.GetValue().ToString() == "1" ? false : true;
                        }
                    });
                }
                Fields.Add(field);
            }
            else if (CheckBoxFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                var field = new CheckBoxFieldControl()
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key
                };
                Fields.Add(field);
            }else if(OptionsSelectFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                var field = new OptionsSelectFieldControl()
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key,
                    IsRequiredField = item.Required
                };
                field.InitControl(item.ValuesAvailable);
                Fields.Add(field);
            }
            else if (TimeFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                var field = new TimeFieldControl()
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key,
                };
                Fields.Add(field);
            }
            else if (OptionsButtonsFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                var field = new OptionsButtonsFieldControl()
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key,
                    IsRequiredField = item.Required
                };
                field.InitControl(item.ValuesAvailable);
                Fields.Add(field);
            }else if (ImageFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                var field = new ImageFieldControl(_nodeService)
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key,
                    IsRequiredField = item.Required
                };
                Fields.Add(field);
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("option") && !query.ContainsKey("fromsummary"))
            {
                IsBusy = true;
                AppOption = query["option"] as AppOptions;

                await RenderForm().ContinueWith((t) =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        IsBusy = false;

                    }
                }).ConfigureAwait(false);

            }
        }


        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task SubmitForm()
        {
            var fieldsEmpty = Fields.Where(x => (x as IFieldControl)?.HasValue() == false && (x as IFieldControl)?.IsRequired() == true);
            if (fieldsEmpty.Count() > 0)
            {
                foreach (var item in fieldsEmpty)
                {
                    var field = item as IFieldControl;
                    field?.SetRequiredStatus();
                }
                await Shell.Current.DisplayAlert("Advertencia", "Debe completar los campos", "Aceptar");
                return;
            }
            IsBusy = true;
            Dictionary<string, List<Node>> values = new();
            values.Add("type", new List<Node>() { new() { TargetId = _form?.FirstOrDefault()?.FieldContentType } });
            foreach (var item in _form?.Where(x => x.Hidden && x.Active))
            {
                values.Add(item.Key, new List<Node>() { new() { Value = item.DefaultValue } });
            }
            values.Add("title", new List<Node>() { new() { Value = $"{AppOption.OptionTitle} {DateTime.Now}" } });
            foreach (var item in Fields)
            {
                var field = item as IFieldControl;
                if (field != null)
                {
                    if (values.ContainsKey(field.Key))
                        continue;
                    if (field.Key == "field_centro_de_votacion")
                    {
                        var val = field.GetValue().ToString();
                        if(val != null)
                        {
                            var request = await _nodeService.GetAllVotingCentersByCode(val, CancellationToken.None);
                            if (request != null)
                            {
                                var _votingCenters = request.data.Select(x => x.attributes).ToList();
                                var targetCdv = _votingCenters.FirstOrDefault(x => x.field_codigo_centro_votacion.ToString() == field.GetValue().ToString() || x.field_codigo_centro_votacion.ToString() == field.GetValue()?.ToString()?.TrimStart('0'));
                                if (targetCdv != null)
                                {
                                    values.Add("field_centro_de_votacion", new List<Node>() { new() { TargetId = targetCdv.drupal_internal__nid } });
                                }
                                else
                                {
                                    await Shell.Current.DisplayAlert("Advertencia", $"No se encontró el centro de votación con el código {field.GetValue()}.", "Aceptar");
                                    IsBusy = false;
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        var value = field.GetValue();
                        if (int.TryParse(value.ToString(), out int valueInteger))
                        {
                            values.Add(field.Key, new List<Node>() { new() { Value = valueInteger } });
                        }
                        else if (value is List<Node> list)
                        {
                            values.Add(field.Key, list);
                        }
                        else
                        {
                            values.Add(field.Key, new List<Node>() { new() { Value = value } });
                        }
                    }

                }

            }

            await SendRequest(values);

            IsBusy = false;
        }

        private async Task SendRequest(Dictionary<string, List<Node>> values)
        {
            IsBusy = true;
            var result = await _nodeService.CreateNode(values, CancellationToken.None);
            if (result != null)
            {
                IsBusy = false;
                await Shell.Current.GoToAsync(nameof(SummaryPageModel), new Dictionary<string, object>()
                {
                    { "message", "Su documento se ha cargado correctamente" },
                    { "type", SummaryPageModel.TYPE_SUCCESS },
                    { "actions", null }
                });
            }
            else
            {
                var actions = new List<ActionButtonDTO>()
                {
                    new ActionButtonDTO("Guardar Offline", "ButtonPrimary", new AsyncRelayCommand(async () =>
                    {
                        var savedNode = new SavedNode()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = AppOption.OptionTitle,
                            SubTitle = DateTime.Now.ToShortDateString(),
                            values = values
                        };

                        var storedSaved = Barrel.Current.Get<List<SavedNode>>($"{nameof(SavedNode)}/reportes/{AppOption.OptionKey}") ?? new();
                        storedSaved.Add(savedNode);

                        Barrel.Current.Add($"{nameof(SavedNode)}/reportes/{AppOption.OptionKey}", storedSaved, TimeSpan.MaxValue);

                        await Shell.Current.Navigation.PopToRootAsync();
                    })),
                    new ActionButtonDTO("Reintentar", "ButtonPrimary", new AsyncRelayCommand(async () =>
                    {
                        await Shell.Current.GoToAsync("..?fromsummary=true");
                        await SendRequest(values);
                    }))
                };
                IsBusy = false;
                await Shell.Current.GoToAsync(nameof(SummaryPageModel), new Dictionary<string, object>()
                {
                    { "message", "Su documento no se pudo cargar, elija una opción" },
                    { "type", SummaryPageModel.TYPE_ERROR },
                    { "actions", actions }
                });
            }
        }
    }
}

