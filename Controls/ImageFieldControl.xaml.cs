using Microsoft.Maui.Graphics.Platform;

namespace ElectoralMonitoring;

public partial class ImageFieldControl : ContentView, IFieldControl
{
    public static List<string> TypesAvailable = new()
    {
       "image_image"
    };

    public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(InputFieldControl), string.Empty, BindingMode.OneTime);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(InputFieldControl), string.Empty);
    public static readonly BindableProperty IsRequiredFieldProperty = BindableProperty.Create(nameof(IsRequiredField), typeof(bool), typeof(InputFieldControl));
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(InputFieldControl));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsRequiredField
    {
        get => (bool)GetValue(IsRequiredFieldProperty);
        set => SetValue(IsRequiredFieldProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public ImageFieldControl(NodeService nodeService)
    {
        _nodeService = nodeService;
        InitializeComponent();
    }

    readonly NodeService _nodeService;
    int _fid = -1;

    public object GetValue()
    {
        return _fid;
    }

    public void SetValue(object value)
    {
        throw new NotImplementedException();
    }

    public bool HasValue()
    {
        return _fid != -1;
    }

    public bool IsRequired()
    {
        return IsRequiredField;
    }

    public string GetKey()
    {
        return Key;
    }

    public void SetRequiredStatus()
    {
        MyBorder.SetDynamicResource(Border.StrokeProperty, "Red");
        RequiredLabel.IsVisible = true;
    }

    public void ClearStatusRequired()
    {
        if (!RequiredLabel.IsVisible) return;
        RequiredLabel.IsVisible = false;
        var dark = Color.FromArgb("#404040");
        var light = Color.FromArgb("#ACACAC");
        MyBorder.SetAppTheme(Border.StrokeProperty, light, dark);
    }

    void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        IsLoading = true;
        TakePhotoCropAndUpload();
        IsLoading = false;
    }

    public void TakePhotoCropAndUpload()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            new ImageCropper.Maui.ImageCropper()
            {
                PageTitle = "Recorte la imagen",
                AspectRatioX = 2,
                MediaPickerOptions = new MediaPickerOptions()
                {
                    Title = "Seleccione"
                },
                AspectRatioY = 3,
                CropShape = ImageCropper.Maui.ImageCropper.CropShapeType.Rectangle,
                SelectSourceTitle = "Seleccione",
                TakePhotoTitle = "Tomar foto",
                PhotoLibraryTitle = "Desde galería",
                CropButtonTitle = "Recortar",
                CancelButtonTitle = "Cancelar",
                Success = async (sourceUri) =>
                {
                    IsLoading = true;
                    int fid = -1;
                    var imageUri = string.Empty;
                    var imageFile = await SaveFileInLocalStorage(sourceUri);
                    var stream = File.OpenRead(imageFile);
                    var fileName = System.IO.Path.GetFileName(imageFile);
                    var result = await _nodeService.UploadMinute(fileName, stream, CancellationToken.None);
                    if (result != null)
                    {
                        if (result.TryGetValue("fid", out List<Node>? value) && value != null)
                        {
                            var fidNode = value.FirstOrDefault();
                            int.TryParse(fidNode?.Value?.ToString(), out fid);
                        }
                        if (result.TryGetValue("uri", out List<Node>? valueUri) && valueUri != null)
                        {
                            imageUri = valueUri.FirstOrDefault()?.Url ?? string.Empty;
                            Console.WriteLine($"ImageURI: {Helpers.AppSettings.BackendUrl + imageUri}");
                        }
                    }

                    // result imageFile and fid, imageUri
                    FieldImage.Source = Helpers.AppSettings.BackendUrl + imageUri;
                    FieldLabel.Text = fileName;
                    _fid = fid; ClearStatusRequired();
                    IsLoading = false;
                },
                Failure = () =>
                {
                    IsLoading = false;
                }
            }.Show(Shell.Current.CurrentPage);
        }
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
                        if (int.TryParse(fidNode?.Value?.ToString(), out fid));
                    }
                    if (result.TryGetValue("uri", out List<Node> valueUri) && value != null)
                    {
                        imageUri = valueUri.FirstOrDefault()?.Url ?? string.Empty;
                    }
                }


                // result imageFile and fid, imageUri
                FieldImage.Source = Helpers.AppSettings.BackendUrl + imageUri;
                _fid = fid; ClearStatusRequired();
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
    static async Task<string> SaveFileInLocalStorage(string photoUri)
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(photoUri) + "2" + System.IO.Path.GetExtension(photoUri);
        string localFilePath = System.IO.Path.Combine(FileSystem.CacheDirectory, name);

        using (var sourceStream = File.OpenRead(photoUri))
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
