using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectoralMonitoring
{
	public partial class ScannerPreviewPageModel : BasePageModel, IQueryAttributable
    {
        [ObservableProperty]
        string imagePreview = string.Empty;

        public ScannerPreviewPageModel(AuthService authService) : base(authService)
        {
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("localFilePath"))
            {
                ImagePreview = query["localFilePath"] as string ?? string.Empty;
            }
        }
    }
}

