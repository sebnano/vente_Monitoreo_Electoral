using System;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Firebase.Functions;

namespace ElectoralMonitoring
{
	public partial class ScannerPreviewPageModel : BasePageModel, IQueryAttributable
    {
        readonly IFirebaseFunctions _firebaseFunctions;
        [ObservableProperty]
        string imagePreview = string.Empty;

        public ScannerPreviewPageModel(AuthService authService, IFirebaseFunctions firebaseFunctions) : base(authService)
        {
            _firebaseFunctions = firebaseFunctions;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("localFilePath") && query.ContainsKey("imageBase64"))
            {
                ImagePreview = query["localFilePath"] as string ?? string.Empty;

                var base64 = query["imageBase64"] as string ?? string.Empty;
                GetContent(base64);
            }
        }

        public async void GetContent(string base64Image)
        {
            try
            {
                var data = new OCRDocumentRequest(base64Image);
                var json = data.ToJson();
                var function = _firebaseFunctions.GetHttpsCallable("imageTextRecognition");

                var response = await function.CallAsync<List<OCRDocumentResponse>>(json);
                Console.WriteLine(response.FirstOrDefault()?.FullTextAnnotation.Text);

                //Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

