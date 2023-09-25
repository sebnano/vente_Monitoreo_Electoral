using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Firebase.Functions;

namespace ElectoralMonitoring
{
	public partial class ScannerPreviewPageModel : BasePageModel, IQueryAttributable
    {
        readonly NodeService _nodeService;
        List<VotingCenter> _votingCenters;

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
            var form = await _nodeService.GetMinutesForm(CancellationToken.None);
            Fields ??= new();
            var fieldsInView = form?.content.Where(x =>
            //campos que se omiten en la creacion del acta
            x.Key != "body" && x.Key != "created" && x.Key != "field_image" && x.Key != "langcode" && x.Key != "path" && x.Key != "promote" && x.Key != "status" && x.Key != "sticky" && x.Key != "title" && x.Key != "uid"
            //campo que se completa automaticamente
            && x.Key != "field_votacion_a_observar"
            //campos que se deben enviar por debajo
            && x.Key != "field_image").ToList();
            if(fieldsInView != null)
            {
                foreach (var item in fieldsInView)
                {
                    Shell.Current.Dispatcher.Dispatch(() =>
                    {
                        var field = new InputFieldControl()
                        {
                            Title = item.Key,
                            FieldType = item.Value.type == "number" ? FieldType.Number : FieldType.Text
                        };
                        Fields.Add(field);
                    });
                }
            }
        }

        async Task LoadVotingCenters()
        {
            var request = await _nodeService.GetVotingCenters(CancellationToken.None);
            if(request != null)
            {
                _votingCenters = request;
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("localFilePath") && query.ContainsKey("image") && query.ContainsKey("imageType"))
            {
                IsBusy = IsLoading = true;
                ImagePreview = query["localFilePath"] as string ?? string.Empty;

                var image = query["image"] as string ?? string.Empty;
                var imageType = (ImageType)query["imageType"];
                await Task.WhenAll(RenderForm(), GetContent(imageType, image), LoadVotingCenters()).ContinueWith(async(t) => {
                    if (t.IsCompletedSuccessfully) {
                        await SetFields();
                    }
                });

                IsBusy = IsLoading = false;
            }
        }

        public Task SetFields()
        {
            try
            {
                SetFieldText("CÓD. CV:", "field_centro_de_votacion", 10);
                SetFieldText("MESA: ", "field_mesa", 2);
                SetFieldText("ESCRUTADAS ", "field_boletas_escrutadas", 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return Task.CompletedTask;
        }

        public void SetFieldText(string searchText, string key, int lenght)
        {
            var indexcdv = TextScanned.IndexOf(searchText);
            if (indexcdv > 0 && indexcdv < TextScanned.Length - 1)
            {
                var cv = TextScanned.Substring(indexcdv + searchText.Length, lenght).Trim();
                var field = Fields.FirstOrDefault(x => (x as InputFieldControl)?.Title == key) as InputFieldControl;
                if (field != null)
                    field.Text = cv;
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
                Console.WriteLine(response.FirstOrDefault()?.FullTextAnnotation.Text);
                TextScanned = response.FirstOrDefault()?.FullTextAnnotation.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task SubmitForm()
        {
            var fieldsEmpty = Fields.Any(x => string.IsNullOrWhiteSpace((x as InputFieldControl)?.Text));
            if (fieldsEmpty) {
                await Shell.Current.DisplayAlert("Advertencia", "Debe completar los campos", "Aceptar");
                return;
            }
            IsLoading = true;
            Dictionary<string, List<Node>> values = new();
            values.Add("type", new List<Node>() { new() { TargetId = "registro_de_actas" } });
            values.Add("title", new List<Node>() { new() { Value = $"ACTA DE ESCRUTINIO {DateTime.Now}" } });
            foreach (var item in Fields)
            {
                var field = item as InputFieldControl;
                if(field != null) {
                    if (values.ContainsKey(field.Title))
                        continue;

                    if (field.Title == "field_centro_de_votacion")
                    {
                        var targetCdv = _votingCenters.FirstOrDefault(x => x.CodCNECentroVotacion == field.Text || x.CodCNECentroVotacion == field.Text.TrimStart('0'));
                        if(targetCdv != null)
                        {
                            values.Add(field.Title, new List<Node>() { new() { TargetId = targetCdv.IdCentroVotacion } });

                            values.Add("field_votacion_a_observar", new List<Node>() { new() { TargetId = targetCdv.IdVotacion } });
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Advertencia", $"No se encontró el centro de votación con el código {field.Text}.", "Aceptar");
                            IsLoading = false;
                            return;
                        }
                        
                    }
                    else
                    {
                        var valueText = field.Text;
                        if(int.TryParse(valueText, out int value))
                        {
                            values.Add(field.Title, new List<Node>() { new() { Value = value } });
                        }
                        else
                        {
                            values.Add(field.Title, new List<Node>() { new() { Value = valueText } });
                        }
                    }
                }
                
            }

            var result = await _nodeService.CreateNode(values, CancellationToken.None);
            if(result != null)
            {
                await Shell.Current.DisplayAlert("Mensaje", "Datos subidos con exito", "Aceptar");
                await Shell.Current.GoToAsync("..");
            }

            IsLoading = false;
        }
    }
}

