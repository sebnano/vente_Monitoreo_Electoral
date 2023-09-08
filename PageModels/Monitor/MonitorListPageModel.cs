using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ElectoralMonitoring
{
    public partial class MonitorListPageModel : BasePageModel
    {
        [ObservableProperty]
        string username;

        public MonitorListPageModel(AuthService authService) : base(authService)
        {
            Username = _authService.NameUser;
        }

        public Microsoft.Maui.Controls.Page ContextPage { get; set; }

        [RelayCommand(AllowConcurrentExecutions = false)]
        public Task TakePhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                new ImageCropper.Maui.ImageCropper()
                {
                    PageTitle = "Recorte la imagen",
                    AspectRatioX = 9,
                    AspectRatioY = 16,
                    CropShape = ImageCropper.Maui.ImageCropper.CropShapeType.Rectangle,
                    SelectSourceTitle = "Seleccione",
                    TakePhotoTitle = "Tomar foto",
                    PhotoLibraryTitle = "Desde galería",
                    CropButtonTitle = "Recortar",
                    CancelButtonTitle = "Cancelar",
                    Success = async (imageFile) =>
                    {
                        var bytes = await File.ReadAllBytesAsync(imageFile);
                        var base64 = Convert.ToBase64String(bytes);
                        var navigationParameter = new Dictionary<string, object>
                        {
                            { "localFilePath", imageFile },
                            { "imageBase64", base64 }
                        };
                        await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter);
                    }
                }.Show(ContextPage);
            }

            return Task.CompletedTask;
        }
    }
}

