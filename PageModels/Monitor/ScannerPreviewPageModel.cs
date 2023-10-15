using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectoralMonitoring.Resources.Lang;
using Plugin.Firebase.Functions;

namespace ElectoralMonitoring
{
    public partial class ScannerPreviewPageModel : BasePageModel, IQueryAttributable
    {
        readonly NodeService _nodeService;
        List<VotingCenter> _votingCenters;
        List<FieldForm>? _form;

        int _fid;
        string _mesa;
        string _ccv;

        [ObservableProperty]
        string imagePreview = string.Empty;

        [ObservableProperty]
        string textScanned = string.Empty;

        [ObservableProperty]
        ObservableCollection<View> fields;

        [ObservableProperty]
        bool isLoading;

        public ScannerPreviewPageModel(NodeService nodeService, AuthService authService) : base(authService)
        {
            _nodeService = nodeService;
        }

        async Task<bool> CheckCanContinue()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return true;

            var mesa = (Fields.FirstOrDefault(x => (x as IFieldControl)?.Key == "field_mesa") as IFieldControl)?.GetValue().ToString();
            var ccv = (Fields.FirstOrDefault(x => (x as IFieldControl)?.Key == "field_centro_de_votacion") as IFieldControl)?.GetValue().ToString();
            if (mesa is null || ccv is null) return false;
            if (_votingCenters != null && _votingCenters.Count > 0)
            {
                var hasAccess = _votingCenters.Any(x => x.CodCNECentroVotacion == ccv || x.CodCNECentroVotacion == ccv.TrimStart('0'));
                if (!hasAccess)
                {
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

        async Task LoadVotingCenters()
        {
            var request = await _nodeService.GetVotingCenters(CancellationToken.None);
            if (request != null)
            {
                _votingCenters = request;
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("mesa") && query.ContainsKey("ccv") && query.ContainsKey("nodeId") && !query.ContainsKey("fromsummary"))
            {

                IsBusy = IsLoading = true;

                _ccv = (string)query["ccv"];
                _mesa = (string)query["mesa"];
                var nodeId = (string)query["nodeId"];

                _ = Task.Run(async () =>
                {
                    await Task.WhenAll(RenderForm(), LoadVotingCenters(), LoadNode(nodeId)).ContinueWith((_) =>
                    {
                        SetFieldTextDirect(_ccv, "field_centro_de_votacion");
                        SetFieldTextDirect(_mesa, "field_mesa");
                        if (NodeToEdit != null)
                        {
                            foreach (var item in NodeToEdit.Where(x => x.Key != "field_centro_de_votacion" && x.Key != "field_mesa" && x.Key != "field_image"))
                            {
                                var value = item.Value.FirstOrDefault()?.Value?.ToString() ?? string.Empty;
                                SetFieldTextDirect(value, item.Key);
                            }

                            var fieldImageNode = NodeToEdit.FirstOrDefault(x => x.Key == "field_image").Value?.FirstOrDefault();
                            var field = Fields.SingleOrDefault(x => (x as IFieldControl)?.Key == "field_image") as IFieldControl;
                            if (field != null && fieldImageNode != null)
                                field.SetValue(fieldImageNode);
                        }


                        IsLoading = IsBusy = false;
                    }).ConfigureAwait(false);
                });
            }
            else if (query.ContainsKey("mesa") && query.ContainsKey("ccv") && !query.ContainsKey("fromsummary"))
            {
                IsBusy = IsLoading = true;

                _ccv = (string)query["ccv"];
                _mesa = (string)query["mesa"];

                _ = Task.Run(async () =>
                {
                    await Task.WhenAll(RenderForm(), LoadVotingCenters()).ContinueWith((_) =>
                    {
                        SetFieldTextDirect(_ccv, "field_centro_de_votacion");
                        SetFieldTextDirect(_mesa, "field_mesa");
                        IsLoading = IsBusy = false;
                    }).ConfigureAwait(false);
                });
            }
        }

        Dictionary<string, List<Node>>? NodeToEdit { get; set; }

        private async Task LoadNode(string nodeId)
        {
            NodeToEdit = await _nodeService.GetNode(nodeId, CancellationToken.None);
        }

        #region Scanner
        public Task SetFields()
        {
            try
            {
                SetFieldTextAsc("PARTICIPANTES ", "PARTICIPANTES ".Length, "field_participantes_segun_cuader", 2);
                SetFieldTextDesc("NULOS", 2, "field_votos_nulos", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return Task.CompletedTask;
        }


        public void SetFieldTextDirect(string text, string key)
        {
            var field = Fields.SingleOrDefault(x => (x as IFieldControl)?.Key == key) as IFieldControl;
            if (field != null)
                field.SetValue(text);

        }

        public void SetFieldTextDesc(string searchText, int marginLeft, string key, int lenght)
        {
            var indexcdv = TextScanned.IndexOf(searchText);
            if (indexcdv > 0)
            {
                var cv = TextScanned.Substring(indexcdv - marginLeft, lenght).Trim();
                var field = Fields.FirstOrDefault(x => (x as IFieldControl)?.Key == key) as IFieldControl;
                if (field != null)
                    field.SetValue(cv);
            }
        }

        public void SetFieldTextAsc(string searchText, int marginRight, string key, int lenght)
        {
            var indexcdv = TextScanned.IndexOf(searchText);
            if (indexcdv > 0 && indexcdv < TextScanned.Length - 1)
            {
                var cv = TextScanned.Substring(indexcdv + marginRight, lenght).Trim();
                var field = Fields.FirstOrDefault(x => (x as IFieldControl)?.Key == key) as IFieldControl;
                if (field != null)
                    field.SetValue(cv);
            }
        }

        public async Task GetContent(ImageType type, string image)
        {
            try
            {
                var documentRequest = new OCRDocumentRequest(type, image);
                var docJsonString = documentRequest.ToJson();
                Console.WriteLine(docJsonString);
                var function = CrossFirebaseFunctions.Current.GetHttpsCallable("imageTextRecognition");
                var response = await function.CallAsync<List<OCRDocumentResponse>>(docJsonString);
                var page = response.FirstOrDefault()?.FullTextAnnotation?.Pages.FirstOrDefault();
                if (page is not null)
                    ScanMinute(page);
                TextScanned = response.FirstOrDefault()?.FullTextAnnotation.Text ?? string.Empty;
                Console.WriteLine(TextScanned);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        List<Word> Words = new();
        void ScanMinute(Page page)
        {
            var pageText = "";
            foreach (var block in page.Blocks)
            {
                var blockText = "";
                foreach (var paragraph in block.Paragraphs)
                {
                    var paraText = "";
                    foreach (var word in paragraph.Words)
                    {
                        var wordText = "";
                        foreach (var symbol in word.Symbols)
                        {
                            wordText += symbol.Text;
                        }
                        Console.WriteLine(
                            string.Format("\nWord text:\n {0}\nWord Confidence: {1})\n",
                            wordText,
                            word.Confidence)
                        );

                        Console.WriteLine(string.Format("Word bounding box: \n{0}", word.BoundingBox));
                        paraText = string.Format("{0} {1}", paraText, wordText);
                        word.Text = wordText;
                        Words.Add(word);
                    }

                    // Console.WriteLine(string.Format("\nParagraph:\n {0}", paraText));
                    // Console.WriteLine(string.Format("Paragraph Confidence: {0}\n", paragraph.Confidence));
                    // Console.WriteLine(string.Format("Paragraph bounding box: \n{0}", paragraph.BoundingBox));
                    blockText += paraText;
                }

                //Console.WriteLine(string.Format("\nBlock:\n {0}", blockText));
                //Console.WriteLine(string.Format("nBlock Confidence: {0}\n", block.Confidence));
                //Console.WriteLine(string.Format("nBlock bounding box: \n{0}", block.BoundingBox));
                pageText += blockText;
            }


            Dictionary<string, string> candidates = new Dictionary<string, string>()
            {
                { "field_votos_candidato_1", "CALECA" },
                { "field_votos_candidato_2", "VELASQUEZ" },
                { "field_votos_candidato_3", "RADONSKI" },
                { "field_votos_candidato_4", "PROSPERI" },
                { "field_votos_candidato_5", "ALMEIDA" },
                { "field_votos_candidato_6", "VIVAS" },
                { "field_votos_candidato_7", "DELSA" },
                { "field_votos_candidato_8", "FREDDY" },
                { "field_votos_candidato_9", "GLORIA" },
                { "field_votos_candidato_10", "FARIAS" },
                { "field_votos_candidato_11", "CORINA" },
                { "field_votos_candidato_12", "ROBERTO" },
                { "field_votos_candidato_13", "TAMARA" },
                { "field_boletas_escrutadas", "ESCRUTADAS" }
            };

            foreach (var cand in candidates)
            {
                var candidate = Words.FirstOrDefault(x => x.Text.ToUpper() == cand.Value.ToUpper());
                if (candidate != null)
                {

                    var votesOfCandidate = Words.FirstOrDefault(x =>
                    x.Text.ToUpper() != candidate.Text.ToUpper()

                    && x.BoundingBox.Vertices[0].Y + 20 >= candidate.BoundingBox.Vertices[0].Y
                    && x.BoundingBox.Vertices[0].Y <= candidate.BoundingBox.Vertices[0].Y + 10

                    && x.BoundingBox.Vertices.Average(x => x.X) >= (candidate.BoundingBox.Vertices.Average(x => x.X) + 100)
                    && x.BoundingBox.Vertices.Average(x => x.X) <= (candidate.BoundingBox.Vertices.Average(x => x.X) + 200)
                    );


                    var field = Fields.FirstOrDefault(x => (x as IFieldControl)?.Key == cand.Key) as IFieldControl;
                    var canScanned = _form?.Any(x => x.NeedScan && x.Key == cand.Key);
                    if (field != null && votesOfCandidate != null)
                        field.SetValue(votesOfCandidate.Text);
                }
            }
        }
        #endregion

        #region Form
        private async Task RenderForm()
        {
            _form = await _nodeService.GetMinutesFormFields(CancellationToken.None);
            Fields ??= new();
            var fieldsObjsToView = _form?.Where(x =>
            x.Key != "field_votacion_a_observar").ToList();

            if (fieldsObjsToView != null)
            {
                await Shell.Current.Dispatcher.DispatchAsync(() =>
                {
                    var grouped = fieldsObjsToView.GroupBy(x => x.Grupo);

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
            }
            else if (OptionsSelectFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                if (item.Key == "field_mesa" && string.IsNullOrEmpty(item.ValuesAvailable))
                {
                    item.ValuesAvailable = "1|1,2|2,3|3,4|4,5|5,6|6,7|7,8|8,9|9,10|10,11|11,12|12,13|13,14|14,15|15,16|16,17|17,18|18,19|19,20|20";
                }
                var field = new OptionsSelectFieldControl()
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key,
                    IsRequiredField = item.Required
                };
                field.InitControl(item.ValuesAvailable ?? string.Empty);
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
                field.InitControl(item.ValuesAvailable ?? string.Empty);
                Fields.Add(field);
            }
            else if (ImageFieldControl.TypesAvailable.Any(x => x == item.Type))
            {
                var field = new ImageFieldControl(_nodeService)
                {
                    Title = item.FieldMapeoTexto,
                    Key = item.Key,
                    IsRequiredField = item.Required
                };
                field.PhotoUploaded += (uri) =>
                {
                    _ = Task.Run(async () =>
                    {
                        var result = await Shell.Current.Dispatcher.DispatchAsync(async() => await Shell.Current.DisplayAlert(AppRes.AlertTitle, "¿Desea escanear los valores del acta?", "SI", "NO"));

                        if (result)
                        {
                            IsBusy = IsLoading = true;
                            await GetContent(ImageType.URI, uri).ContinueWith(async (t) =>
                            {
                                if (t.IsCompletedSuccessfully)
                                {
                                    await SetFields();

                                    IsBusy = IsLoading = false;
                                }
                            }).ConfigureAwait(false);
                        }
                    });
                };
                
                Fields.Add(field);
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
            IsLoading = true;
            Dictionary<string, List<Node>> values = new();
            values.Add("type", new List<Node>() { new() { TargetId = "registro_de_actas" } });
            values.Add("title", new List<Node>() { new() { Value = $"ACTA DE ESCRUTINIO {DateTime.Now}" } });
            foreach (var item in Fields)
            {
                var field = item as IFieldControl;
                if (field != null)
                {
                    if (values.ContainsKey(field.Key))
                        continue;

                    if (field.Key == "field_centro_de_votacion")
                    {
                        var targetCdv = _votingCenters.FirstOrDefault(x => x.CodCNECentroVotacion == field.GetValue().ToString() || x.CodCNECentroVotacion == field.GetValue()?.ToString()?.TrimStart('0'));
                        if (targetCdv != null)
                        {
                            values.Add("field_centro_de_votacion", new List<Node>() { new() { TargetId = targetCdv.IdCentroVotacion } });

                            values.Add("field_votacion_a_observar", new List<Node>() { new() { TargetId = targetCdv.IdVotacion } });
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Advertencia", $"No se encontró el centro de votación con el código {field.GetValue()}.", "Aceptar");
                            IsLoading = false;
                            return;
                        }

                    }
                    else
                    {
                        var value = field.GetValue();
                        if (int.TryParse(value.ToString(), out int valueInteger))
                        {
                            if (field.Key == "field_image")
                            {
                                values.Add("field_image", new List<Node>() { new() { TargetId = valueInteger } });
                            }
                            else
                            {
                                values.Add(field.Key, new List<Node>() { new() { Value = valueInteger } });
                            }
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

            IsLoading = false;
        }

        private async Task SendRequest(Dictionary<string, List<Node>> values)
        {
            IsLoading = true;
            var can = await CheckCanContinue();
            if (!can)
            {
                IsLoading = false;
                return;
            }
            Task<Dictionary<string,List<Node>>?>? request = null;
            if(NodeToEdit != null)
            {
                var nid = NodeToEdit["nid"].FirstOrDefault()?.Value;
                values.Add("nid", new List<Node>() { new() { Value = nid } });
                request = _nodeService.EditNode(nid?.ToString() ?? string.Empty, values, CancellationToken.None);
            }
            else
            {
                request = _nodeService.CreateNode(values, CancellationToken.None);
            }
            var result = await request;
            if (result != null)
            {
                IsLoading = false;
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
                    new ActionButtonDTO("Guardar Offline", "ButtonPrimary", new AsyncRelayCommand(async () => await Shell.Current.Navigation.PopToRootAsync())),
                    new ActionButtonDTO("Reintentar", "ButtonPrimary", new AsyncRelayCommand(async () =>
                    {
                        await Shell.Current.GoToAsync("..?fromsummary=true");
                        await SendRequest(values);
                    }))
                };
                IsLoading = false;
                await Shell.Current.GoToAsync(nameof(SummaryPageModel), new Dictionary<string, object>()
                {
                    { "message", "Su documento no se pudo cargar, elija una opción" },
                    { "type", SummaryPageModel.TYPE_ERROR },
                    { "actions", actions }
                });
            }
        }
        #endregion
    }
}

