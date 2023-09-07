using System;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Firebase.Firestore.Util;
using Plugin.Firebase.Functions;
using static Android.Icu.Text.CaseMap;

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
            if (query.ContainsKey("localFilePath") && query.ContainsKey("base64"))
            {
                ImagePreview = query["localFilePath"] as string ?? string.Empty;
                var base64 = query["base64"] as string ?? string.Empty;
                GetContent(base64);
            }
        }

        public async void GetContent(string imageBase64)
        {
            try
            {
                Console.WriteLine("Base64 Image:");
                Console.WriteLine(imageBase64);
                var json = new OCRDocumentRequest(imageBase64).ToJson();
                var response = await _firebaseFunctions.GetHttpsCallable("imageTextRecognition")
                    .CallAsync<dynamic>(json);//OCRDocumentResponse
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

