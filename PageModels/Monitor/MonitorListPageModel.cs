using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectoralMonitoring.Resources.Lang;
using Microsoft.Maui.Graphics.Platform;
using Plugin.Firebase.Storage;

namespace ElectoralMonitoring
{
    public partial class MonitorListPageModel : BasePageModel
    {
        string ccv;
        string mesa;

        readonly NodeService _nodeService;

        [ObservableProperty]
        string username;

        [ObservableProperty]
        bool isAdding;

        [ObservableProperty]
        ObservableCollection<Minute>? minutes;

        public MonitorListPageModel(NodeService nodeService, AuthService authService) : base(authService)
        {
            _nodeService = nodeService;
            Username = _authService.NameUser;
            AuthService.NamedChanged += AuthService_NamedChanged;
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
                    Minutes = new(list.Select(x => new Minute()
                    {
                        field_centro_de_votacion = x.field_centro_de_votacion,
                        field_mesa = x.field_mesa,
                        nid = x.nid,
                        Icon = IconFont.FileDocumentCheck
                    }));
                }
                IsBusy = false;
            }
        }

        private void AuthService_NamedChanged(object? sender, EventArgs e)
        {
            Username = _authService.NameUser;
            _ = Init();
        }

        public Microsoft.Maui.Controls.Page? ContextPage { get; set; }

        async Task<bool> CheckCanContinue()
        {
            var cvs = await _nodeService.GetVotingCenters(CancellationToken.None);
            if(cvs != null && cvs.Count > 0)
            {
                var hasAccess = cvs.Any(x => x.CodCNECentroVotacion == ccv || x.CodCNECentroVotacion == ccv.TrimStart('0'));
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
        public async Task TakePhoto()
        {
            ccv = await Shell.Current.DisplayPromptAsync("Código del centro de votación", "Ingrese el código para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "010101001", 9, Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(ccv)) return;

            mesa = await Shell.Current.DisplayPromptAsync("Mesa", "Ingrese el número de mesa para continuar", AppRes.AlertAccept, AppRes.AlertCancel, "01", 2, Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(mesa)) return;

            IsAdding = true;
            var can = await CheckCanContinue();
            if (!can)
            {
                IsAdding = false;
                return;
            }

            await Shell.Current.DisplayAlert("Mensaje", "A continuación debe agregar la foto del acta", "OK");

            await TakePhotoCropAndUpload().ConfigureAwait(false);
        }

        public Task TakePhotoCropAndUpload()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                new ImageCropper.Maui.ImageCropper()
                {
                    PageTitle = "Recorte la imagen",
                    AspectRatioX = 2,
                    AspectRatioY = 3,
                    CropShape = ImageCropper.Maui.ImageCropper.CropShapeType.Rectangle,
                    SelectSourceTitle = "Seleccione",
                    TakePhotoTitle = "Tomar foto",
                    PhotoLibraryTitle = "Desde galería",
                    CropButtonTitle = "Recortar",
                    CancelButtonTitle = "Cancelar",
                    Success = async (imageFile) =>
                    {
                        int fid = -1;
                        var imageUri = string.Empty;

                        var stream = File.OpenRead(imageFile);
                        var fileName = System.IO.Path.GetFileName(imageFile);
                        var result = await _nodeService.UploadMinute(fileName, stream, CancellationToken.None);
                        if (result != null)
                        {
                            if (result.TryGetValue("fid", out List<Node> value) && value != null)
                            {
                                var fidNode = value.FirstOrDefault();
                                if (int.TryParse(fidNode?.Value?.ToString(), out fid)) ;
                            }
                            if (result.TryGetValue("uri", out List<Node> valueUri) && value != null)
                            {
                                imageUri = valueUri.FirstOrDefault()?.Url ?? string.Empty;
                                Console.WriteLine($"ImageURI: {Helpers.AppSettings.BackendUrl+imageUri}");
                            }
                        }

                        var navigationParameter = new Dictionary<string, object>
                        {
                            { "localFilePath", imageFile },
                            { "image", Helpers.AppSettings.BackendUrl+imageUri },
                            { "imageType", ImageType.URI },
                            { "fileId", fid },
                            { "ccv", ccv },
                            { "mesa", mesa },
                        };
                        IsAdding = false;
                        await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter);

                    },
                    Failure = () =>
                    {
                        IsAdding = false;
                    }
                }.Show(ContextPage);
            }

            return Task.CompletedTask;
        }

        public async Task TakePhotoAndUpload()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    var imageFile = await SaveFileInLocalStorage(photo);

                    int fid = -1;
                    var imageUri = string.Empty;

                    var stream = File.OpenRead(imageFile);

                    var fileName = System.IO.Path.GetFileName(imageFile);
                    var result = await _nodeService.UploadMinute(fileName, stream, CancellationToken.None);
                    if (result != null)
                    {
                        if (result.TryGetValue("fid", out List<Node> value) && value != null)
                        {
                            var fidNode = value.FirstOrDefault();
                            if (int.TryParse(fidNode?.Value?.ToString(), out fid)) ;
                        }
                        if (result.TryGetValue("uri", out List<Node> valueUri) && value != null)
                        {
                            imageUri = valueUri.FirstOrDefault()?.Url ?? string.Empty;
                        }
                    }

                    var navigationParameter = new Dictionary<string, object>
                        {
                            { "localFilePath", imageFile },
                            { "image", Helpers.AppSettings.BackendUrl+imageUri },
                            { "imageType", ImageType.URI },
                            { "fileId", fid }
                        };
                    IsAdding = false;
                    await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter);

                }
            }
        }

        static async Task<string> SaveFileInLocalStorage(FileResult photo)
        {
            string localFilePath = System.IO.Path.Combine(FileSystem.CacheDirectory, photo.FileName);

            using (var sourceStream = await photo.OpenReadAsync())
            {
                using (var localFileStream = File.OpenWrite(localFilePath))
                {
                    var image = PlatformImage.FromStream(sourceStream);
                    if (image != null)
                    {
                        var newImage = image.Downsize(1000, 2000, true);
                        await newImage.SaveAsync(localFileStream);
                    }
                }
            }

            return localFilePath;
        }
    }
}

