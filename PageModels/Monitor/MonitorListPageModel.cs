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

        [RelayCommand]
        public async Task TakePhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);

                    await sourceStream.CopyToAsync(localFileStream);

                    var memoryStream = new MemoryStream();

                    await localFileStream.CopyToAsync(memoryStream);
                    var base64 = Convert.ToBase64String(memoryStream.ToArray());

                    var navigationParameter = new Dictionary<string, object>
                    {
                        { "localFilePath", localFilePath },
                        { "base64", base64 }
                    };
                    await Shell.Current.GoToAsync(nameof(ScannerPreviewPageModel), navigationParameter);
                }
            }
        }
    }
}

