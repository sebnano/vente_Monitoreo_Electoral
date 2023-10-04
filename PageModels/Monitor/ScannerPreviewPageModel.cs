using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Firebase.Functions;

namespace ElectoralMonitoring
{
    public partial class ScannerPreviewPageModel : BasePageModel, IQueryAttributable
    {
        readonly NodeService _nodeService;
        List<VotingCenter> _votingCenters;
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

        private async Task RenderForm()
        {
            var form = await _nodeService.GetMinutesFormFields(CancellationToken.None);
            Fields ??= new();
            var fieldsObjsToView = form?.Where(x =>
            //campos que se omiten en la creacion del acta
            x.Key != "body" && x.Key != "created" && x.Key != "langcode" && x.Key != "path" && x.Key != "promote" && x.Key != "status" && x.Key != "sticky" && x.Key != "title" && x.Key != "uid"
            //campos que se deben enviar por debajo
            && x.Key != "field_votacion_a_observar"
            && x.Key != "field_image").ToList();

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
                            if (InputFieldControl.TypesAvailable.Any(x => x == item.Type))
                            {
                                // Faltaria campo de picker de centros de votacion, mesa
                                var field = new InputFieldControl()
                                {
                                    Title = item.FieldMapeoTexto,
                                    Key = item.Key,
                                    FieldType = item.Type == FieldForm.NUMBER ? FieldType.Number : FieldType.Text,
                                    MaxLenght = item.Key == "field_observaciones" ? -1 : 100
                                };
                                Fields.Add(field);
                            }
                        }

                    }
                }).ConfigureAwait(false);

            }
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

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("localFilePath") && query.ContainsKey("image") && query.ContainsKey("imageType")
                && query.ContainsKey("fileId") && query.ContainsKey("mesa") && query.ContainsKey("ccv") && !query.ContainsKey("fromsummary"))
            {
                IsBusy = IsLoading = true;
                ImagePreview = query["localFilePath"] as string ?? string.Empty;

                _fid = (int)query["fileId"];
                _ccv = (string)query["ccv"];
                _mesa = (string)query["mesa"];

                var image = query["image"] as string ?? string.Empty;
                var imageType = (ImageType)query["imageType"];

                _ = Task.Run(async () =>
                {

                    await Task.WhenAll(RenderForm(), GetContent(imageType, image), LoadVotingCenters()).ContinueWith(async (t) =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            await SetFields();

                            IsBusy = IsLoading = false;
                        }
                    }).ConfigureAwait(false);
                });
            }
            else if (query.ContainsKey("fromsummary"))
            {

            }
        }

        public Task SetFields()
        {
            try
            {
                //todo verificar que datos ingresados de ccv y mesa coincidan con la foto
                SetFieldTextAsc("CÓD. CV:", "CÓD. CV:".Length, "field_centro_de_votacion", 10);
                SetFieldTextAsc("MESA: ", "MESA: ".Length, "field_mesa", 2);
                SetFieldTextAsc("PARTICIPANTES ", "PARTICIPANTES ".Length, "field_participantes_segun_cuader", 2);
                SetFieldTextDesc("VOTOS", 2, "field_votos_nulos", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return Task.CompletedTask;
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
                    if (field != null && votesOfCandidate != null)
                        field.SetValue(votesOfCandidate.Text);
                }
            }
        }


        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task SubmitForm()
        {
            var fieldsEmpty = Fields.Any(x => (x as IFieldControl)?.HasValue() == false);
            if (fieldsEmpty)
            {
                await Shell.Current.DisplayAlert("Advertencia", "Debe completar los campos", "Aceptar");
                return;
            }
            IsLoading = true;
            Dictionary<string, List<Node>> values = new();
            values.Add("type", new List<Node>() { new() { TargetId = "registro_de_actas" } });
            values.Add("title", new List<Node>() { new() { Value = $"ACTA DE ESCRUTINIO {DateTime.Now}" } });
            values.Add("field_image", new List<Node>() { new() { TargetId = _fid } });
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
                        var valueText = field.GetValue().ToString();
                        if (int.TryParse(valueText, out int value))
                        {
                            values.Add(field.Key, new List<Node>() { new() { Value = value } });
                        }
                        else
                        {
                            values.Add(field.Key, new List<Node>() { new() { Value = valueText } });
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

            var result = await _nodeService.CreateNode(values, CancellationToken.None);
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
    }
}

