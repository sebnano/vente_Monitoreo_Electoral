using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Firebase.Functions;

namespace ElectoralMonitoring
{
	public partial class ScannerPreviewPageModel : BasePageModel, IQueryAttributable
    {
        [ObservableProperty]
        string imagePreview = string.Empty;

        [ObservableProperty]
        string textScanned = string.Empty;

        public ScannerPreviewPageModel(AuthService authService) : base(authService)
        {
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("localFilePath") && query.ContainsKey("image") && query.ContainsKey("imageType"))
            {
                ImagePreview = query["localFilePath"] as string ?? string.Empty;

                var image = query["image"] as string ?? string.Empty;
                var imageType = (ImageType)query["imageType"];
                GetContent(imageType, image);
            }
        }

        public async void GetContent(ImageType type, string image)
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
    }
}

