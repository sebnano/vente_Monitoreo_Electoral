using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics.Platform;
using Plugin.Firebase.Storage;

namespace ElectoralMonitoring
{
    public partial class MonitorListPageModel : BasePageModel
    {
        [ObservableProperty]
        string username;

        [ObservableProperty]
        ObservableCollection<object> minutes;

        public MonitorListPageModel(AuthService authService) : base(authService)
        {
            Username = _authService.NameUser;
            Minutes = new();
            AuthService.NamedChanged += AuthService_NamedChanged;
        }

        private void AuthService_NamedChanged(object? sender, EventArgs e)
        {
            Username = _authService.NameUser;
        }

        public Microsoft.Maui.Controls.Page? ContextPage { get; set; }

        [RelayCommand(AllowConcurrentExecutions = false)]
        public Task TakePhoto()
        {
#if IOS
            return TakePhotoCropAndUpload();
#endif
#if ANDROID
            return TakePhotoCropAndUpload();
#endif

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
                    Success = (imageFile) =>
                    {
                        var rootRef = CrossFirebaseStorage.Current.GetRootReference();
                        var fileName = Path.GetFileName(imageFile);
                        var imageRef = rootRef.GetChild($"app/actas/{fileName}");
                        var uploadTask = imageRef.PutFile(imageFile);
                        uploadTask.AddObserver(StorageTaskStatus.Success, async (_) =>
                        {
                            var imageUri = $"gs://{_.Metadata.Bucket}/{_.Metadata.Path}";
                            var navigationParameter = new Dictionary<string, object>
                            {
                                { "localFilePath", imageFile },
                                { "image", imageUri },
                                { "imageType", ImageType.URI }
                            };
                            await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter);
                        });

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
                    var localFilePath = await SaveFileInLocalStorage(photo);

                    var rootRef = CrossFirebaseStorage.Current.GetRootReference();
                    var fileName = photo.FileName;
                    var imageRef = rootRef.GetChild($"app/actas/{fileName}");
                    var uploadTask = imageRef.PutFile(localFilePath);
                    uploadTask.AddObserver(StorageTaskStatus.Success, async (_) =>
                    {
                        var imageUri = $"gs://{_.Metadata.Bucket}/{_.Metadata.Path}"; 
                        var navigationParameter = new Dictionary<string, object>
                        {
                            { "localFilePath", localFilePath },
                            { "image", imageUri },
                            { "imageType", ImageType.URI }
                        };
                        await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter);
                    });
                    
                }
            }
        }

        static async Task<string> SaveFileInLocalStorage(FileResult photo)
        {
            string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

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

